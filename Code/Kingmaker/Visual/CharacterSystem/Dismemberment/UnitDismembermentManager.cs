using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View;
using Kingmaker.Visual.CharactersRigidbody;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Particles;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment;

public class UnitDismembermentManager : MonoBehaviour
{
	[Serializable]
	public class DismembermentBone
	{
		public Transform Transform;

		public float SliceOffset;

		public Vector3 SliceOrientationEuler;
	}

	[Serializable]
	public class DismembermentPrefabDescriptor
	{
		public PrefabLink PrefabLink;

		public List<DismembermentPieceDescriptor> Pieces = new List<DismembermentPieceDescriptor>();
	}

	[Serializable]
	public class DismembermentSet
	{
		public DismembermentLimbsApartType Type;

		public List<DismembermentBone> SliceBones = new List<DismembermentBone>();

		public PrefabLink Prefab;
	}

	public class BoneDataObsolete : IHashable
	{
		[JsonProperty(IsReference = false)]
		public Vector3 Position;

		[JsonProperty(IsReference = false)]
		public Quaternion Rotation;

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(ref Position);
			result.Append(ref Rotation);
			return result;
		}
	}

	public int SetIndex = -1;

	public int DestroyedPieceIndex = -1;

	public GameObject DismembermentGameObjectToFx;

	public List<DismembermentSet> Sets = new List<DismembermentSet>();

	private bool m_Dismembered;

	private Transform m_MainPieceRootBone;

	public GameObject visualEffectPrefab;

	private RigidbodyCreatureController.ImpulseData m_LastImpulse;

	public Dictionary<GameObject, GameObject> AdditionalEquipment = new Dictionary<GameObject, GameObject>();

	public bool NeedToRebakeDismemberment;

	public float ImpulseMax = 40f;

	public bool Dismembered => m_Dismembered;

	public Transform MainPieceRootBone => m_MainPieceRootBone;

	public RigidbodyCreatureController.ImpulseData LastImpulse => m_LastImpulse;

	public void ApplyImpulse(Vector3 direction, float additionalMagnitude, DamageType damageType)
	{
		m_LastImpulse = m_LastImpulse ?? new RigidbodyCreatureController.ImpulseData();
		m_LastImpulse.Direction = direction;
		m_LastImpulse.MagnitudeModifier = additionalMagnitude;
		m_LastImpulse.Time = Game.Instance.TimeController.RealTime;
		m_LastImpulse.DamageType = damageType;
	}

	public void StartDismemberment([NotNull] StatefulRandom random, DismembermentLimbsApartType? dismemberingType = null)
	{
		if (m_LastImpulse != null && !(Game.Instance.TimeController.RealTime > m_LastImpulse.Time + 0.2f.Seconds()) && m_LastImpulse.DamageType != DamageType.Toxic)
		{
			StartDismemberment(random, GetSetIndex(random, dismemberingType), m_LastImpulse.Direction);
		}
	}

	private int GetSetIndex(StatefulRandom random, DismembermentLimbsApartType? dismemberingType)
	{
		if (!dismemberingType.HasValue)
		{
			return random.Range(0, Sets.Count);
		}
		int num = Sets.FindIndex((DismembermentSet d) => d.Type == dismemberingType.Value);
		if (num == -1)
		{
			return random.Range(0, Sets.Count);
		}
		return num;
	}

	private void PrepareBludgeonDismemberment(StatefulRandom random, DismembermentLimbsApartType? dismemberingType)
	{
		if (dismemberingType.HasValue)
		{
			int num = Sets.FindIndex((DismembermentSet d) => d.Type == dismemberingType.Value);
			int setIndex = ((num != -1) ? num : random.Range(0, Sets.Count));
			StartDismemberment(random, setIndex, m_LastImpulse.Direction, null, bludgeon: true);
			return;
		}
		List<DismembermentSet> list = Sets.Where((DismembermentSet y) => y.SliceBones.Count == 1 && y.Type != DismembermentLimbsApartType.Body).ToList();
		if (list.Any())
		{
			DismembermentSet item = list[random.Range(0, list.Count)];
			int setIndex = Sets.IndexOf(item);
			StartDismemberment(random, setIndex, m_LastImpulse.Direction, null, bludgeon: true);
		}
	}

	public void StartDismemberment(StatefulRandom random, int setIndex, Vector3 impulse, GameObject set = null, bool bludgeon = false)
	{
		SetIndex = setIndex;
		GameObject gameObject;
		if (set == null)
		{
			set = Sets[setIndex].Prefab.Load();
			gameObject = UnityEngine.Object.Instantiate(set, base.transform);
		}
		else
		{
			gameObject = set;
		}
		DismembermentGameObjectToFx = gameObject;
		DismembermentSetBehaviour component = gameObject.GetComponent<DismembermentSetBehaviour>();
		if (component == null)
		{
			PFLog.TechArt.Error("Set behaviour not found.");
			return;
		}
		for (int i = 0; i < component.Pieces.Count; i++)
		{
			Transform skeleton = component.Pieces[i].Skeleton;
			if (skeleton != null)
			{
				skeleton.gameObject.SetActive(value: false);
			}
		}
		if (bludgeon)
		{
			for (int j = 0; j < component.Pieces.Count; j++)
			{
				if (j > 0)
				{
					DestroyedPieceIndex = j;
					UnityEngine.Object.DestroyImmediate(component.Pieces[j].Root.gameObject);
				}
			}
			ParticlesSnapMap component2 = gameObject.GetComponent<ParticlesSnapMap>();
			List<FxBone> list = new List<FxBone>();
			for (int k = 0; k < component2.Bones.Count(); k++)
			{
				FxBone fxBone = component2.Bones[k];
				if (fxBone.Transform != null)
				{
					list.Add(fxBone);
				}
			}
			component2.Bones = list;
		}
		m_MainPieceRootBone = component.Pieces[0].Skeleton.GetChild(0);
		Animator componentInChildren = GetComponentInChildren<Animator>(includeInactive: true);
		if (componentInChildren == null)
		{
			PFLog.TechArt.Error("Can't find Animator, set object " + gameObject.name);
			return;
		}
		componentInChildren.enabled = false;
		SkinnedMeshRenderer componentInChildren2 = componentInChildren.GetComponentInChildren<SkinnedMeshRenderer>();
		if (componentInChildren2 == null)
		{
			PFLog.TechArt.Error("Can't find SkinnedMeshRenderer, set object " + gameObject.name);
			return;
		}
		componentInChildren2.enabled = false;
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(componentInChildren.transform);
		while (stack.Count > 0)
		{
			Transform transform = stack.Pop();
			if (!dictionary.ContainsKey(transform.name))
			{
				dictionary.Add(transform.name, transform);
			}
			int childCount = transform.childCount;
			for (int l = 0; l < childCount; l++)
			{
				stack.Push(transform.GetChild(l));
			}
		}
		List<KeyValuePair<Rigidbody, Rigidbody>> list2 = new List<KeyValuePair<Rigidbody, Rigidbody>>();
		List<Rigidbody> list3 = new List<Rigidbody>();
		List<Collider> list4 = new List<Collider>();
		List<CharacterJoint> list5 = new List<CharacterJoint>();
		List<SkinnedMeshRenderer> list6 = new List<SkinnedMeshRenderer>();
		component.Pieces[0].Root.GetComponentInChildren<SkinnedMeshRenderer>();
		Material material = ((component.SliceMaterial != null) ? new Material(component.SliceMaterial) : null);
		for (int m = 0; m < component.Pieces.Count; m++)
		{
			if (bludgeon && m > 0)
			{
				continue;
			}
			DismembermentPieceDescriptor dismembermentPieceDescriptor = component.Pieces[m];
			Transform skeleton2 = dismembermentPieceDescriptor.Skeleton;
			if (!(skeleton2 != null))
			{
				continue;
			}
			skeleton2.gameObject.SetActive(value: true);
			list3.AddRange(dismembermentPieceDescriptor.ImpulseRigidBodies);
			list4.AddRange(dismembermentPieceDescriptor.Colliders);
			list5.AddRange(dismembermentPieceDescriptor.Joints);
			for (int n = 0; n < dismembermentPieceDescriptor.ImpulseRigidBodies.Length; n++)
			{
				Rigidbody rigidbody = dismembermentPieceDescriptor.ImpulseRigidBodies[n];
				if (componentInChildren != null && dictionary.TryGetValue(rigidbody.transform.name, out var value))
				{
					rigidbody.position = value.position;
					rigidbody.rotation = value.rotation;
				}
				float mass = dismembermentPieceDescriptor.ImpulseRigidBodies[n].mass;
				float num = random.Range(dismembermentPieceDescriptor.ImpulseMultiplier.x, dismembermentPieceDescriptor.ImpulseMultiplier.y);
				float num2 = random.Range(dismembermentPieceDescriptor.IncomingImpulseMultiplier.x, dismembermentPieceDescriptor.IncomingImpulseMultiplier.y) * 10f;
				float num3 = random.Range(dismembermentPieceDescriptor.ChildrenImpulseMultiplier.x, dismembermentPieceDescriptor.ChildrenImpulseMultiplier.y);
				Vector3 vector = dismembermentPieceDescriptor.ImpulseRigidBodies[0].transform.TransformDirection(dismembermentPieceDescriptor.Impulse);
				if (n == 0)
				{
					Vector3 vector2 = vector * num * mass + impulse * num2;
					vector2 = Vector3.ClampMagnitude(vector2, ImpulseMax);
					rigidbody.AddForce(vector2, ForceMode.Impulse);
				}
				else
				{
					Vector3 vector3 = (vector * num * mass + impulse * num2) * num3;
					vector3 = Vector3.ClampMagnitude(vector3, ImpulseMax);
					rigidbody.AddForce(vector3, ForceMode.Impulse);
				}
				SkinnedMeshRenderer componentInChildren3 = dismembermentPieceDescriptor.Root.GetComponentInChildren<SkinnedMeshRenderer>();
				if (!(componentInChildren3 != null))
				{
					continue;
				}
				if (material != null)
				{
					Material[] sharedMaterials = componentInChildren3.sharedMaterials;
					for (int num4 = 0; num4 < sharedMaterials.Length; num4++)
					{
						if (sharedMaterials[num4].name.Contains(material.name))
						{
							sharedMaterials[num4] = material;
						}
					}
					componentInChildren3.sharedMaterials = sharedMaterials;
				}
				componentInChildren3.enabled = false;
				list6.Add(componentInChildren3);
			}
		}
		componentInChildren2.enabled = true;
		Dictionary<string, Transform> dictionary2 = new Dictionary<string, Transform>();
		PBDSkinnedBody[] componentsInChildren = componentInChildren.GetComponentsInChildren<PBDSkinnedBody>();
		foreach (PBDSkinnedBody pBDSkinnedBody in componentsInChildren)
		{
			dictionary2.Add(pBDSkinnedBody.transform.parent.name, pBDSkinnedBody.transform);
		}
		foreach (KeyValuePair<string, Transform> item in dictionary2)
		{
			foreach (DismembermentPieceDescriptor piece in component.Pieces)
			{
				Transform transform2 = piece.Skeleton.FindChildRecursive(item.Key);
				if (transform2 != null && (transform2.GetComponent<Rigidbody>() != null || transform2.name == "Pelvis_ADJ"))
				{
					Vector3 localPosition = item.Value.localPosition;
					Quaternion localRotation = item.Value.localRotation;
					item.Value.parent = transform2;
					item.Value.localPosition = localPosition;
					item.Value.localRotation = localRotation;
					break;
				}
			}
		}
		int num6 = 0;
		for (int num7 = 0; num7 < list3.Count - 1; num7++)
		{
			for (int num8 = num6 + 1; num8 < list3.Count; num8++)
			{
				if (list3[num7].name == list3[num8].name)
				{
					list2.Add(new KeyValuePair<Rigidbody, Rigidbody>(list3[num7], list3[num8]));
					break;
				}
			}
			num6 = num7 + 1;
		}
		for (int num9 = 0; num9 < list2.Count; num9++)
		{
			Collider component3 = list2[num9].Key.GetComponent<Collider>();
			Collider component4 = list2[num9].Value.GetComponent<Collider>();
			if (component3 != null && component4 != null)
			{
				Physics.IgnoreCollision(component3, component4);
			}
		}
		UnitEntityView component5 = GetComponent<UnitEntityView>();
		if (component5 != null && component5.Blueprint != null)
		{
			visualEffectPrefab = component5.Blueprint.VisualSettings.GetDismember(component5.EntityData.SurfaceType, UnitDismemberType.LimbsApart);
			if (material != null)
			{
				HitEntry[] hitEffects = BlueprintRoot.Instance.HitSystemRoot.HitEffects;
				foreach (HitEntry hitEntry in hitEffects)
				{
					if (hitEntry.Type == component5.EntityData.SurfaceType)
					{
						material.color = hitEntry.CreaturesHitEffect.BloodColor;
						break;
					}
				}
			}
		}
		component = gameObject.GetComponent<DismembermentSetBehaviour>();
		component.GetComponent<SnapMapBase>().Init();
		if ((bool)componentInChildren)
		{
			componentInChildren.gameObject.SetActive(value: true);
			FxLocator[] componentsInChildren2 = componentInChildren.gameObject.GetComponentsInChildren<FxLocator>(includeInactive: true);
			for (int num5 = 0; num5 < componentsInChildren2.Length; num5++)
			{
				componentsInChildren2[num5].gameObject.SetActive(value: false);
			}
			componentInChildren.gameObject.SetActive(value: false);
		}
		SnapMapBase component6 = component5.gameObject.GetComponent<SnapMapBase>();
		if ((bool)component6)
		{
			component6.Init();
		}
		RagdollRecieverMain component7 = gameObject.GetComponent<RagdollRecieverMain>();
		if ((bool)component7)
		{
			component7.FindRagdollSender();
		}
		Game.Instance.CoroutinesController.Start(PostProcess(componentInChildren.gameObject, list6.ToArray(), visualEffectPrefab, component.gameObject));
		StartCoroutine(RagdollStop(list3, list4, list5, 14));
		m_Dismembered = true;
	}

	private IEnumerator PostProcess(GameObject animator, SkinnedMeshRenderer[] piecesSkinnedMeshRenderers, GameObject visualEffect, GameObject effectTarget)
	{
		yield return null;
		animator.SetActive(value: false);
		foreach (GameObject value in AdditionalEquipment.Values)
		{
			if (value != null)
			{
				value.SetActive(value: false);
			}
		}
		for (int i = 0; i < piecesSkinnedMeshRenderers.Length; i++)
		{
			piecesSkinnedMeshRenderers[i].enabled = true;
		}
		if (visualEffect != null)
		{
			FxHelper.SpawnFxOnGameObject(visualEffect, effectTarget);
		}
	}

	private IEnumerator RagdollStop(List<Rigidbody> rigidbodies, List<Collider> colliders, List<CharacterJoint> joints, int timeToStop)
	{
		for (int timer = 0; timer < timeToStop; timer++)
		{
			bool flag = true;
			foreach (Rigidbody rigidbody in rigidbodies)
			{
				if (!(rigidbody == null) && !rigidbody.IsSleeping())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				break;
			}
			yield return new WaitForSeconds(1f);
		}
		DestroyPhysics(rigidbodies, colliders, joints);
	}

	private void DestroyPhysics(List<Rigidbody> rigidbodies, List<Collider> colliders, List<CharacterJoint> joints)
	{
		foreach (Collider collider in colliders)
		{
			if (collider != null)
			{
				UnityEngine.Object.DestroyImmediate(collider);
			}
		}
		foreach (CharacterJoint joint in joints)
		{
			if (joint != null)
			{
				UnityEngine.Object.DestroyImmediate(joint);
			}
		}
		foreach (Rigidbody rigidbody in rigidbodies)
		{
			if (rigidbody != null)
			{
				UnityEngine.Object.DestroyImmediate(rigidbody);
			}
		}
	}

	public void RestoreState()
	{
		if (SetIndex < 0)
		{
			return;
		}
		bool flag = DestroyedPieceIndex >= 0;
		GameObject original = Sets[SetIndex].Prefab.Load();
		original = UnityEngine.Object.Instantiate(original, base.transform);
		DismembermentSetBehaviour component = original.GetComponent<DismembermentSetBehaviour>();
		if (component == null)
		{
			PFLog.TechArt.Error("Set behaviour not found.");
			return;
		}
		if (flag)
		{
			UnityEngine.Object.DestroyImmediate(component.Pieces[DestroyedPieceIndex].Root.gameObject);
			ParticlesSnapMap component2 = original.GetComponent<ParticlesSnapMap>();
			List<FxBone> list = new List<FxBone>();
			for (int i = 0; i < component2.Bones.Count(); i++)
			{
				FxBone fxBone = component2.Bones[i];
				if (fxBone.Transform != null)
				{
					list.Add(fxBone);
				}
			}
			component2.Bones = list;
		}
		m_MainPieceRootBone = component.Pieces[0].Skeleton.GetChild(0);
		Animator componentInChildren = GetComponentInChildren<Animator>(includeInactive: true);
		if (componentInChildren == null)
		{
			PFLog.TechArt.Error("Can't find Animator, set object " + original.name);
			return;
		}
		componentInChildren.enabled = false;
		SkinnedMeshRenderer componentInChildren2 = componentInChildren.GetComponentInChildren<SkinnedMeshRenderer>();
		if (componentInChildren2 == null)
		{
			PFLog.TechArt.Error("Can't find SkinnedMeshRenderer, set object " + original.name);
			return;
		}
		componentInChildren2.enabled = false;
		List<KeyValuePair<Rigidbody, Rigidbody>> list2 = new List<KeyValuePair<Rigidbody, Rigidbody>>();
		List<Rigidbody> list3 = new List<Rigidbody>();
		List<Collider> list4 = new List<Collider>();
		List<CharacterJoint> list5 = new List<CharacterJoint>();
		List<SkinnedMeshRenderer> list6 = new List<SkinnedMeshRenderer>();
		Material material = ((component.SliceMaterial != null) ? new Material(component.SliceMaterial) : null);
		for (int j = 0; j < component.Pieces.Count; j++)
		{
			if (flag && j > 0)
			{
				continue;
			}
			DismembermentPieceDescriptor dismembermentPieceDescriptor = component.Pieces[j];
			Transform skeleton = dismembermentPieceDescriptor.Skeleton;
			if (!(skeleton != null))
			{
				continue;
			}
			skeleton.gameObject.SetActive(value: true);
			list3.AddRange(dismembermentPieceDescriptor.ImpulseRigidBodies);
			list4.AddRange(dismembermentPieceDescriptor.Colliders);
			list5.AddRange(dismembermentPieceDescriptor.Joints);
			for (int k = 0; k < dismembermentPieceDescriptor.ImpulseRigidBodies.Length; k++)
			{
				SkinnedMeshRenderer componentInChildren3 = dismembermentPieceDescriptor.Root.GetComponentInChildren<SkinnedMeshRenderer>();
				if (!(componentInChildren3 != null))
				{
					continue;
				}
				if (material != null)
				{
					Material[] sharedMaterials = componentInChildren3.sharedMaterials;
					for (int l = 0; l < sharedMaterials.Length; l++)
					{
						if (sharedMaterials[l].name.Contains(material.name))
						{
							sharedMaterials[l] = material;
						}
					}
					componentInChildren3.sharedMaterials = sharedMaterials;
				}
				componentInChildren3.enabled = false;
				list6.Add(componentInChildren3);
			}
		}
		componentInChildren2.enabled = true;
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
		PBDSkinnedBody[] componentsInChildren = componentInChildren.GetComponentsInChildren<PBDSkinnedBody>();
		foreach (PBDSkinnedBody pBDSkinnedBody in componentsInChildren)
		{
			dictionary.Add(pBDSkinnedBody.transform.parent.name, pBDSkinnedBody.transform);
		}
		foreach (KeyValuePair<string, Transform> item in dictionary)
		{
			foreach (DismembermentPieceDescriptor piece in component.Pieces)
			{
				Transform transform = piece.Skeleton.FindChildRecursive(item.Key);
				if (transform != null && (transform.GetComponent<Rigidbody>() != null || transform.name == "Pelvis_ADJ"))
				{
					Vector3 localPosition = item.Value.localPosition;
					Quaternion localRotation = item.Value.localRotation;
					item.Value.parent = transform;
					item.Value.localPosition = localPosition;
					item.Value.localRotation = localRotation;
					break;
				}
			}
		}
		int num = 0;
		for (int n = 0; n < list3.Count - 1; n++)
		{
			for (int num2 = num + 1; num2 < list3.Count; num2++)
			{
				if (list3[n].name == list3[num2].name)
				{
					list2.Add(new KeyValuePair<Rigidbody, Rigidbody>(list3[n], list3[num2]));
					break;
				}
			}
			num = n + 1;
		}
		for (int num3 = 0; num3 < list2.Count; num3++)
		{
			Collider component3 = list2[num3].Key.GetComponent<Collider>();
			Collider component4 = list2[num3].Value.GetComponent<Collider>();
			if (component3 != null && component4 != null)
			{
				Physics.IgnoreCollision(component3, component4);
			}
		}
		UnitEntityView component5 = GetComponent<UnitEntityView>();
		if (component5 != null && component5.Blueprint != null)
		{
			visualEffectPrefab = component5.Blueprint.VisualSettings.GetDismember(component5.EntityData.SurfaceType, UnitDismemberType.LimbsApart);
			if (material != null)
			{
				HitEntry[] hitEffects = BlueprintRoot.Instance.HitSystemRoot.HitEffects;
				foreach (HitEntry hitEntry in hitEffects)
				{
					if (hitEntry.Type == component5.EntityData.SurfaceType)
					{
						material.color = hitEntry.CreaturesHitEffect.BloodColor;
						break;
					}
				}
			}
		}
		Game.Instance.CoroutinesController.Start(PostProcess(componentInChildren.gameObject, list6.ToArray(), visualEffectPrefab, component.gameObject));
		DestroyPhysics(list3, list4, list5);
		m_Dismembered = true;
	}
}
