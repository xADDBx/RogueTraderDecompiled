using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Particles;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Visual.CharactersRigidbody;

public class RigidbodyCreatureController : MonoBehaviour, IUpdatable
{
	public class ImpulseData
	{
		public Vector3 Direction;

		public float MagnitudeModifier;

		public TimeSpan Time;

		public DamageType DamageType;
	}

	public enum RagdollState
	{
		Off,
		Falling,
		Lying,
		Standing
	}

	[Serializable]
	public struct BoneImpulseMultiplier
	{
		public Rigidbody bone;

		public float multiplier;
	}

	public class BoneData : IHashable
	{
		[JsonProperty]
		public string Name = "";

		[JsonProperty(IsReference = false)]
		public Vector3 Positions;

		[JsonProperty(IsReference = false)]
		public Quaternion Rotations;

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(Name);
			result.Append(ref Positions);
			result.Append(ref Rotations);
			return result;
		}
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("RigidbodyCreatureController");

	[HideInInspector]
	public bool RagdollOnlyOnDeath = true;

	public GameObject RootBone;

	public List<BoneImpulseMultiplier> BoneImpulseMultipliers = new List<BoneImpulseMultiplier>();

	public bool RandomNegativeValueOnMultiplier;

	public float BaseImpulseValue = 10f;

	public float AdditionalImpulseMin;

	public float AdditionalImpulseMax;

	public float MultiplyVectorYAxis = 1f;

	public float InProneMultiplier = 1f;

	public float ImpulseValueMultiplierToParents;

	public float ImpulseValueMultiplierToChildren;

	public bool ApplyImpulseToAllBones;

	[Tooltip("Character objects with rigidbody component")]
	public List<Rigidbody> RigidBones = new List<Rigidbody>();

	[Tooltip("Time after ragdoll simulation will stop")]
	public float RagdollTime = 3f;

	public bool CheckForEarlyStopRagdoll;

	public float MinRootPosition;

	public float MinAllPosition;

	public float MinTimeToStop = 9f;

	[HideInInspector]
	[Tooltip("Main bones which should be returned to source positions on creature stand up")]
	public List<Transform> BonesToReturn = new List<Transform>();

	public AbstractUnitEntityView EntityView;

	private TimeSpan m_StartTime;

	private Vector3 m_PreviousRootPosition;

	private Vector3 m_CurRootPosition;

	private TimeSpan m_StartTimeToStop;

	private readonly List<BoneData> m_BonesData = new List<BoneData>();

	public List<BoneData> RagdollCurrentPositions = new List<BoneData>();

	private ImpulseData m_LastImpulse;

	public bool PostEventWithSurface = true;

	public List<RagdollPostEventWithSurface> EventTargets = new List<RagdollPostEventWithSurface>();

	private RagdollState m_State;

	private Vector3 m_DeathPoint;

	public float minRagdollValue = 1f;

	public float maxRagdollValue = 80f;

	private GameObject m_ActiveRagdoll;

	private List<GameObject> m_ActiveRagdollChildrens = new List<GameObject>();

	public List<GameObject> skeletonBones;

	public RagdollState State
	{
		get
		{
			return m_State;
		}
		private set
		{
			if (m_State != value)
			{
				m_State = value;
				base.enabled = value == RagdollState.Falling;
				Animator component = GetComponent<Animator>();
				if (component != null)
				{
					component.enabled = value == RagdollState.Off;
				}
				UnitAnimationManager component2 = GetComponent<UnitAnimationManager>();
				if (component2 != null)
				{
					component2.Disabled = !component.enabled;
				}
				if (value == RagdollState.Falling || value == RagdollState.Lying)
				{
					PrepareRenderersForRagdoll();
				}
			}
		}
	}

	public bool IsControllingRigidbody
	{
		get
		{
			if (State != RagdollState.Falling)
			{
				return State == RagdollState.Standing;
			}
			return true;
		}
	}

	public bool RagdollWorking => State == RagdollState.Falling;

	public bool IsActive => State != RagdollState.Off;

	public bool IsRagdollPositionsRestored { get; private set; }

	public void SaveBonesPosition(List<BoneData> targetData)
	{
		targetData.Clear();
		foreach (Rigidbody rigidBone in RigidBones)
		{
			if ((bool)base.transform)
			{
				BoneData item = new BoneData
				{
					Name = rigidBone.name,
					Positions = rigidBone.transform.position,
					Rotations = rigidBone.transform.rotation
				};
				targetData.Add(item);
			}
		}
	}

	private void SwitchKinematic(bool enable)
	{
		foreach (Rigidbody rigidBone in RigidBones)
		{
			if (!rigidBone.isKinematic)
			{
				rigidBone.velocity = Vector3.zero;
			}
			rigidBone.ResetCenterOfMass();
			rigidBone.detectCollisions = !enable;
			rigidBone.isKinematic = enable;
			rigidBone.collisionDetectionMode = ((!enable) ? CollisionDetectionMode.Continuous : CollisionDetectionMode.ContinuousSpeculative);
		}
	}

	public void InitRigidbodyCreatureController()
	{
		EntityView = GetComponentInParent<AbstractUnitEntityView>();
		if (EntityView == null)
		{
			Logger.Error(this, "No EntityView found {0}", this);
			return;
		}
		if (EntityView.RigidbodyController == null)
		{
			EntityView.RigidbodyController = this;
		}
		base.enabled = false;
		State = RagdollState.Off;
		if (!RootBone)
		{
			Logger.Error(this, "No root bone in RagdollCharacter {0}", this);
			return;
		}
		if (RigidBones.Count <= 0)
		{
			Logger.Error(this, "No rigid bones in RagdollCharacter {0}", this);
			return;
		}
		EntityView.GetComponent<SnapMapBase>().Init();
		RigidBones.RemoveAll((Rigidbody b) => b == null);
		foreach (Rigidbody rigidBone in RigidBones)
		{
			if (rigidBone.gameObject.layer != 9 && rigidBone.gameObject.layer != 29)
			{
				Logger.Error(this, "Rigidbone {0} without Unit layer. Headwalking will occur. {1}", rigidBone.name, this);
				rigidBone.gameObject.layer = 9;
			}
			rigidBone.ResetInertiaTensor();
			rigidBone.solverIterations = 0;
			rigidBone.maxDepenetrationVelocity = 1f;
			rigidBone.solverVelocityIterations = 0;
			rigidBone.maxAngularVelocity = 0f;
			rigidBone.detectCollisions = false;
			rigidBone.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
			rigidBone.isKinematic = true;
		}
		SwitchRigidbodiesSleep(enable: false);
	}

	public void CancelRagdoll()
	{
		StopRagdoll();
		State = RagdollState.Off;
	}

	public void ReturnToAnimationState()
	{
		StopRagdoll();
		State = RagdollState.Standing;
	}

	private void SwitchRigidbodies(bool enable)
	{
		foreach (Rigidbody rigidBone in RigidBones)
		{
			rigidBone.gameObject.SetActive(enable);
		}
	}

	private void SwitchRigidbodiesSleep(bool enable)
	{
		foreach (Rigidbody rigidBone in RigidBones)
		{
			if (!enable)
			{
				rigidBone.Sleep();
			}
			else
			{
				rigidBone.WakeUp();
			}
			Collider component = rigidBone.GetComponent<Collider>();
			if ((bool)component)
			{
				component.enabled = enable;
			}
		}
	}

	public void StopRagdoll()
	{
		if (State == RagdollState.Falling)
		{
			SwitchKinematic(enable: true);
			SwitchRigidbodies(enable: false);
			SaveBonesPosition(RagdollCurrentPositions);
			base.gameObject.GetComponent<HumanoidRagdollManager>()?.Enabled(flag: false);
		}
		UnitAnimationManager component = GetComponent<UnitAnimationManager>();
		if (component != null && component.IsProne)
		{
			UnitAnimationActionHandle unitAnimationActionHandle = GetComponent<UnitAnimationManager>()?.ExclusiveHandle;
			unitAnimationActionHandle?.Action.OnUpdate(unitAnimationActionHandle, 0.1f);
		}
		State = RagdollState.Lying;
	}

	public void StartRagdoll()
	{
		if ((bool)RootBone && RigidBones.Count > 0 && State != RagdollState.Falling)
		{
			InitBakedCharactersBonesPosition();
			State = RagdollState.Falling;
			m_DeathPoint = base.transform.position;
			SaveBonesPosition(m_BonesData);
			SwitchRigidbodies(enable: true);
			SwitchKinematic(enable: false);
			SwitchRigidbodiesSleep(enable: true);
			m_StartTime = Game.Instance.TimeController.GameTime;
			base.gameObject.GetComponent<HumanoidRagdollManager>()?.Enabled(flag: true);
			if (m_LastImpulse != null && Game.Instance.TimeController.GameTime - m_LastImpulse.Time < 0.2f.Seconds())
			{
				ApplyImpulseDirectly(m_LastImpulse.Direction, m_LastImpulse.MagnitudeModifier);
			}
		}
	}

	public void ApplyImpulse(Vector3 direction, float additionalMagnitude)
	{
		if (State == RagdollState.Falling)
		{
			ApplyImpulseDirectly(direction, additionalMagnitude);
			return;
		}
		m_LastImpulse = m_LastImpulse ?? new ImpulseData();
		m_LastImpulse.Direction = direction;
		m_LastImpulse.MagnitudeModifier = additionalMagnitude;
		m_LastImpulse.Time = Game.Instance.TimeController.GameTime;
	}

	private void ApplyImpulseDirectly(Vector3 direction, float additionalMagnitude)
	{
		if (BoneImpulseMultipliers == null || BoneImpulseMultipliers.Count == 0)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		int num4 = 1;
		if (RandomNegativeValueOnMultiplier && PFStatefulRandom.Visuals.Rigidbody.value > 0.5f)
		{
			num4 = -1;
		}
		float multiplyVectorYAxis;
		float impulseValueMultiplierToChildren;
		float impulseValueMultiplierToParents;
		if (ApplyImpulseToAllBones)
		{
			foreach (BoneImpulseMultiplier boneImpulseMultiplier in BoneImpulseMultipliers)
			{
				Rigidbody bone = boneImpulseMultiplier.bone;
				num = ((0f != boneImpulseMultiplier.multiplier) ? boneImpulseMultiplier.multiplier : 1f);
				AbstractUnitEntity entityData = EntityView.EntityData;
				num3 = ((entityData != null && entityData.State.IsProne) ? InProneMultiplier : 1f);
				num2 = (BaseImpulseValue + PFStatefulRandom.Visuals.Rigidbody.Range(AdditionalImpulseMin, AdditionalImpulseMax)) * (float)num4 * (1f + additionalMagnitude) * num * num3;
				num2 = Mathf.Clamp(num2, minRagdollValue, maxRagdollValue);
				Vector3 impulseDirection = num2 * direction;
				impulseValueMultiplierToParents = ImpulseValueMultiplierToParents;
				impulseValueMultiplierToChildren = ImpulseValueMultiplierToChildren;
				multiplyVectorYAxis = MultiplyVectorYAxis;
				ApplyImpulseToRagdoll(bone, impulseDirection, default(Vector3), zeroVerticalVector: false, default(Vector3), impulseValueMultiplierToParents, impulseValueMultiplierToChildren, multiplyVectorYAxis);
			}
			return;
		}
		int num5 = PFStatefulRandom.Visuals.Rigidbody.Range(0, BoneImpulseMultipliers.Count);
		Rigidbody bone2 = BoneImpulseMultipliers[num5].bone;
		num = ((num5 < BoneImpulseMultipliers.Count) ? BoneImpulseMultipliers[num5].multiplier : 1f);
		AbstractUnitEntity entityData2 = EntityView.EntityData;
		num3 = ((entityData2 != null && entityData2.State.IsProne) ? InProneMultiplier : 1f);
		num2 = (BaseImpulseValue + PFStatefulRandom.Visuals.Rigidbody.Range(AdditionalImpulseMin, AdditionalImpulseMax)) * (float)num4 * (1f + additionalMagnitude) * num * num3;
		num2 = Mathf.Clamp(num2, minRagdollValue, maxRagdollValue);
		Vector3 impulseDirection2 = num2 * direction;
		multiplyVectorYAxis = ImpulseValueMultiplierToParents;
		impulseValueMultiplierToChildren = ImpulseValueMultiplierToChildren;
		impulseValueMultiplierToParents = MultiplyVectorYAxis;
		ApplyImpulseToRagdoll(bone2, impulseDirection2, default(Vector3), zeroVerticalVector: false, default(Vector3), multiplyVectorYAxis, impulseValueMultiplierToChildren, impulseValueMultiplierToParents);
	}

	public static void ApplyImpulseToRagdoll([NotNull] Rigidbody impulseTarget, Vector3 impulseDirection, Vector3 additionalDirection = default(Vector3), bool zeroVerticalVector = false, Vector3 torque = default(Vector3), float impulseValueMultiplierToParents = 0f, float impulseValueMultiplierToChildren = 0f, float multiplyVectorYAxis = 1f)
	{
		if (zeroVerticalVector)
		{
			impulseDirection = new Vector3(impulseDirection.x, 0f, impulseDirection.z);
		}
		impulseDirection = new Vector3(impulseDirection.x, impulseDirection.y * multiplyVectorYAxis, impulseDirection.z);
		impulseDirection += additionalDirection * impulseDirection.magnitude;
		impulseTarget.AddForce(impulseDirection, ForceMode.Impulse);
		if (torque != Vector3.zero)
		{
			impulseTarget.AddTorque(torque.x, torque.y, torque.z);
		}
		Rigidbody rigidbody = impulseTarget;
		float num = impulseValueMultiplierToParents;
		if (impulseValueMultiplierToParents > Mathf.Epsilon)
		{
			while ((bool)rigidbody)
			{
				rigidbody = rigidbody.transform.parent.GetComponent<Rigidbody>();
				if (rigidbody != null)
				{
					rigidbody.AddForce(impulseDirection * num, ForceMode.Impulse);
					num *= impulseValueMultiplierToParents;
				}
			}
		}
		if (impulseValueMultiplierToChildren > Mathf.Epsilon)
		{
			rigidbody = impulseTarget;
			Vector3 childImpulseVec = impulseDirection * impulseValueMultiplierToChildren;
			ImpulseChildren(rigidbody, childImpulseVec, impulseValueMultiplierToChildren);
		}
	}

	private static void ImpulseChildren(Rigidbody curBone, Vector3 childImpulseVec, float childImpulseValueMultiplier)
	{
		int childCount = curBone.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Rigidbody component = curBone.transform.GetChild(i).GetComponent<Rigidbody>();
			if (!(component == null))
			{
				component.AddForce(childImpulseVec, ForceMode.Impulse);
				ImpulseChildren(component, childImpulseVec * childImpulseValueMultiplier, childImpulseValueMultiplier);
			}
		}
	}

	public void RestoreRagdollPositions()
	{
		if (RagdollCurrentPositions.Count <= 0)
		{
			return;
		}
		foreach (BoneData ragdollCurrentPosition in RagdollCurrentPositions)
		{
			foreach (Rigidbody rigidBone in RigidBones)
			{
				if (ragdollCurrentPosition.Name == rigidBone.name)
				{
					Transform obj = rigidBone.transform;
					Quaternion rotation = (rigidBone.rotation = ragdollCurrentPosition.Rotations);
					obj.rotation = rotation;
					Transform obj2 = rigidBone.transform;
					Vector3 position = (rigidBone.position = ragdollCurrentPosition.Positions);
					obj2.position = position;
					break;
				}
			}
		}
		GetComponent<HumanoidRagdollManager>().Or(null)?.CopyPoseFromRagdoll();
		State = RagdollState.Lying;
		SwitchRigidbodies(enable: true);
		SwitchKinematic(enable: false);
		IsRagdollPositionsRestored = true;
	}

	private bool ReturnBonesToAnimatePosition()
	{
		if (BonesToReturn.Count <= 0)
		{
			return true;
		}
		SwitchKinematic(enable: true);
		bool flag = false;
		bool flag2 = false;
		foreach (Transform item in BonesToReturn)
		{
			BoneData bone = GetBone(item.transform);
			if (bone == null)
			{
				Logger.Error(this, "Bone data is null. Ragdoll stopped. {0}", this);
				return false;
			}
			if (item.transform.rotation != bone.Rotations)
			{
				Transform obj = item.transform;
				Quaternion rotation = (item.rotation = Quaternion.RotateTowards(item.transform.rotation, bone.Rotations, 80f * Game.Instance.TimeController.GameDeltaTime));
				obj.rotation = rotation;
				flag = false;
			}
			else
			{
				flag = true;
			}
		}
		foreach (Transform item2 in BonesToReturn)
		{
			BoneData bone2 = GetBone(item2.transform);
			if (bone2 == null)
			{
				Logger.Error(this, "Bone data is null. Ragdoll stopped. {0}", this);
				return false;
			}
			if (item2.transform.position != bone2.Positions)
			{
				Transform obj2 = item2.transform;
				Vector3 position = (item2.position = Vector3.MoveTowards(item2.transform.position, bone2.Positions, 2f * Game.Instance.TimeController.GameDeltaTime));
				obj2.position = position;
				flag2 = false;
			}
			else
			{
				flag2 = true;
			}
		}
		return flag && flag2;
	}

	private BoneData GetBone(Transform targetBone)
	{
		BoneData result = null;
		foreach (BoneData bonesDatum in m_BonesData)
		{
			if (targetBone.name == bonesDatum.Name)
			{
				result = bonesDatum;
			}
		}
		return result;
	}

	private void OnEnable()
	{
		Game.Instance.UpdateRigidbodyCreatureController.Add(this);
	}

	private void OnDisable()
	{
		Game.Instance.UpdateRigidbodyCreatureController.Remove(this);
	}

	void IUpdatable.Tick(float delta)
	{
		if (!EntityView)
		{
			Logger.Error(this, "{0} EntityView link is empty. Abort!", this);
			return;
		}
		if (State == RagdollState.Standing && ReturnBonesToAnimatePosition())
		{
			State = RagdollState.Off;
		}
		if (Game.Instance.TimeController.GameTime > m_StartTime + RagdollTime.Seconds())
		{
			MaybeSpawnLootAtDeathPoint();
			StopRagdoll();
		}
		if (CheckForEarlyStopRagdoll)
		{
			bool num = CheckRagdollIsSlow();
			m_PreviousRootPosition = RootBone.transform.position;
			if (num)
			{
				if (Game.Instance.TimeController.GameTime > m_StartTimeToStop + MinTimeToStop.Seconds())
				{
					MaybeSpawnLootAtDeathPoint();
					StopRagdoll();
				}
			}
			else
			{
				m_StartTimeToStop = Game.Instance.TimeController.GameTime;
			}
		}
		if (State == RagdollState.Lying)
		{
			MaybeSpawnLootAtDeathPoint();
		}
		AbstractUnitEntity entityData = EntityView.EntityData;
		if (entityData != null && !entityData.State.IsAnimating)
		{
			StopRagdoll();
		}
	}

	private void MaybeSpawnLootAtDeathPoint()
	{
		if (EntityView == null || EntityView.EntityData == null || !EntityView.EntityData.IsDeadAndHasLoot || m_DeathPoint == Vector3.zero)
		{
			return;
		}
		float num = float.MaxValue;
		foreach (Rigidbody rigidBone in RigidBones)
		{
			float sqrMagnitude = (rigidBone.transform.position - m_DeathPoint).sqrMagnitude;
			if (num > sqrMagnitude)
			{
				num = sqrMagnitude;
			}
		}
		if (num > BlueprintRoot.Instance.HitSystemRoot.RagdollDistanceForLootBag * BlueprintRoot.Instance.HitSystemRoot.RagdollDistanceForLootBag)
		{
			EntityView.EntityData.GetOptional<PartInventory>()?.DropLootToGround(dismember: false, m_DeathPoint, dropAttached: true);
		}
	}

	private bool CheckRagdollIsSlow()
	{
		Vector3 vector = RootBone.transform.position - m_PreviousRootPosition;
		bool flag = Mathf.Max(Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y)), Mathf.Abs(vector.z)) <= MinRootPosition * (Game.Instance.TimeController.GameDeltaTime * 30f);
		if (flag)
		{
			float num = MinAllPosition * 30f * (MinAllPosition * 30f);
			foreach (Rigidbody rigidBone in RigidBones)
			{
				if (rigidBone.velocity.sqrMagnitude > num)
				{
					flag = false;
					break;
				}
			}
		}
		return flag;
	}

	public void InitBakedCharactersBonesPosition()
	{
		if (skeletonBones.Count == 0)
		{
			skeletonBones = GetAllChildrenInGO(base.gameObject);
		}
		if (m_ActiveRagdoll != null)
		{
			foreach (GameObject item in m_ActiveRagdollChildrens)
			{
				item.SetActive(value: true);
				GameObject gameObject = skeletonBones.FirstOrDefault((GameObject x) => x.name == item.name);
				if (gameObject != null)
				{
					Transform transform = gameObject.transform;
					item.transform.position = transform.position;
					item.transform.rotation = transform.rotation;
				}
			}
			return;
		}
		if (RigidBones.Count != 1000)
		{
			return;
		}
		foreach (Rigidbody rigidBone in RigidBones)
		{
			m_ActiveRagdollChildrens.Add(rigidBone.gameObject);
		}
		SkinnedMeshRenderer componentInChildren = EntityView.GetComponentInChildren<SkinnedMeshRenderer>();
		foreach (GameObject item in m_ActiveRagdollChildrens)
		{
			Transform transform2 = componentInChildren.bones.FirstOrDefault((Transform x) => x.name == item.name);
			item.SetActive(value: true);
			if (transform2 != null)
			{
				Transform transform3 = transform2.transform;
				item.transform.position = transform3.position;
				item.transform.rotation = transform3.rotation;
			}
		}
	}

	public void SetActiveRagdollGO(GameObject ragdoll_right_handed_go, List<GameObject> skeletonBones1)
	{
		m_ActiveRagdoll = ragdoll_right_handed_go;
		foreach (Rigidbody rigidBone in m_ActiveRagdoll.GetComponent<RigidbodyCreatureController>().RigidBones)
		{
			m_ActiveRagdollChildrens.Add(rigidBone.gameObject);
		}
		skeletonBones = skeletonBones1;
	}

	public void FindSkeleton()
	{
		skeletonBones = GetAllChildrenInGO(base.gameObject);
	}

	private List<GameObject> GetAllChildrenInGO(GameObject gameObject)
	{
		Transform transform = gameObject.transform;
		Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
		List<GameObject> list = new List<GameObject>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform2 in array)
		{
			if (transform != transform2)
			{
				list.Add(transform2.gameObject);
			}
		}
		return list;
	}

	private void PrepareRenderersForRagdoll()
	{
		if (!Application.isPlaying || RootBone == null || EntityView == null)
		{
			return;
		}
		SkinnedMeshRenderer componentInChildren = EntityView.GetComponentInChildren<SkinnedMeshRenderer>();
		if (!(componentInChildren == null))
		{
			Mesh sharedMesh = componentInChildren.sharedMesh;
			if (!(sharedMesh == null))
			{
				componentInChildren.rootBone = RootBone.transform;
				Vector3 extents = sharedMesh.bounds.extents;
				float num = Mathf.Max(Mathf.Max(extents.x, extents.y), extents.z);
				float num2 = 2f * num;
				componentInChildren.localBounds = new Bounds(default(Vector3), new Vector3(num2, num2, num2));
			}
		}
	}
}
