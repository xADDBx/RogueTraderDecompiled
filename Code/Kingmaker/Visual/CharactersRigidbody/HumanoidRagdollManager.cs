using System.Collections.Generic;
using Kingmaker.Controllers;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

namespace Kingmaker.Visual.CharactersRigidbody;

[RequireComponent(typeof(Animator))]
public class HumanoidRagdollManager : MonoBehaviour, IUpdatable
{
	public GameObject StandardRagdollSkeleton;

	public GameObject LeftHandedRagdollSkeleton;

	private GameObject m_ActiveRagdoll;

	private bool m_UpdatePoseFlag;

	private List<(Transform bone, Transform ragdollBone)> m_RagdollLinks;

	public List<GameObject> skeletonBones;

	private void OnEnable()
	{
		Game.Instance.CustomLateUpdateController.Add(this);
	}

	private void OnDisable()
	{
		Game.Instance.CustomLateUpdateController.Remove(this);
	}

	public void InitHumanoidRagdoll()
	{
		m_RagdollLinks = new List<(Transform, Transform)>();
		Character componentInParent = base.gameObject.GetComponentInParent<Character>();
		if (componentInParent == null)
		{
			PFLog.TechArt.Error("Cant find Character Script. Ragdoll may work incorrectly.");
		}
		if (StandardRagdollSkeleton == null || LeftHandedRagdollSkeleton == null)
		{
			PFLog.TechArt.Error("Ragdoll Skeleton prefabs are not found in Humanoid Ragdoll Manager fields. Check all fields and add necessary prefabs: " + base.gameObject.transform.parent.name);
			return;
		}
		m_ActiveRagdoll = Object.Instantiate(StandardRagdollSkeleton, base.gameObject.transform.parent, worldPositionStays: false);
		componentInParent?.UpdateSkeletonDirectly(m_ActiveRagdoll.transform);
		RigidbodyCreatureController rigidbodyCreatureController = base.gameObject.AddComponent<RigidbodyCreatureController>();
		RigidbodyCreatureController component = m_ActiveRagdoll.GetComponent<RigidbodyCreatureController>();
		if (component == null)
		{
			PFLog.TechArt.Error("No Rigidbody Creature Controller found on ragdoll skeleton : " + m_ActiveRagdoll.name + ". Check ragdoll skeleton configuration.");
			return;
		}
		rigidbodyCreatureController.GetCopyOfComponent(component);
		rigidbodyCreatureController.InitRigidbodyCreatureController();
		if (component != null)
		{
			Object.Destroy(component);
		}
		LinkJoints(m_ActiveRagdoll.transform, base.transform);
		m_ActiveRagdoll.SetActive(value: true);
		rigidbodyCreatureController.SetActiveRagdollGO(m_ActiveRagdoll, skeletonBones);
	}

	private static Transform FindDeepChild(Transform aParent, string aName)
	{
		Queue<Transform> queue = new Queue<Transform>();
		queue.Enqueue(aParent);
		while (queue.Count > 0)
		{
			Transform transform = queue.Dequeue();
			if (transform.name == aName)
			{
				return transform;
			}
			foreach (Transform item in transform)
			{
				queue.Enqueue(item);
			}
		}
		return null;
	}

	void IUpdatable.Tick(float delta)
	{
		if (m_UpdatePoseFlag && !m_RagdollLinks.Empty() && (bool)m_ActiveRagdoll)
		{
			float num = delta / (float)RealTimeController.SystemStepTimeSpan.TotalSeconds;
			for (int i = 0; i < m_RagdollLinks.Count; i++)
			{
				(Transform bone, Transform ragdollBone) tuple = m_RagdollLinks[i];
				Transform item = tuple.bone;
				Transform item2 = tuple.ragdollBone;
				float maxDistanceDelta = num * Vector3.Distance(item.position, item2.position);
				item.position = Vector3.MoveTowards(item.position, item2.position, maxDistanceDelta);
				float maxDegreesDelta = num * Quaternion.Angle(item.rotation, item2.rotation);
				item.rotation = Quaternion.RotateTowards(item.rotation, item2.rotation, maxDegreesDelta);
			}
		}
	}

	public void CopyPoseFromRagdoll()
	{
		GetPose(m_RagdollLinks);
		UpdatePose(m_RagdollLinks);
	}

	private void GetPose(List<(Transform, Transform)> ragdollLink)
	{
		foreach (var item2 in ragdollLink)
		{
			Transform item = item2.Item1;
			Rigidbody component = item2.Item2.GetComponent<Rigidbody>();
			component.position = item.position;
			component.rotation = item.rotation;
		}
	}

	private void UpdatePose(List<(Transform, Transform)> ragdollLink)
	{
		foreach (var item3 in ragdollLink)
		{
			Transform item = item3.Item1;
			Transform item2 = item3.Item2;
			item.position = item2.position;
			item.rotation = item2.rotation;
		}
	}

	private void LinkJoints(Transform source, Transform destination)
	{
		Rigidbody[] componentsInChildren = source.GetComponentsInChildren<Rigidbody>(includeInactive: true);
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			string aName = rigidbody.name;
			Transform transform = FindDeepChild(destination, aName);
			if (transform != null)
			{
				m_RagdollLinks.Add((transform, rigidbody.transform));
			}
		}
	}

	public void Enabled(bool flag)
	{
		if (flag)
		{
			GetPose(m_RagdollLinks);
		}
		m_UpdatePoseFlag = flag;
	}

	private void AddEffectsEvents()
	{
		if (!(m_ActiveRagdoll == null))
		{
			GameObject gameObject = m_ActiveRagdoll.GetComponentsInChildren<Transform>(includeInactive: true).FirstOrDefault((Transform t) => t.name == "Pelvis")?.gameObject;
			GameObject gameObject2 = m_ActiveRagdoll.GetComponentsInChildren<Transform>(includeInactive: true).FirstOrDefault((Transform t) => t.name == "Spine_3")?.gameObject;
			if (gameObject != null && gameObject2 != null)
			{
				gameObject.AddComponent<RagdollPostEventWithSurface>().GetInfoAboutCharacter();
				gameObject2.AddComponent<RagdollPostEventWithSurface>().GetInfoAboutCharacter();
			}
		}
	}

	public void FindSkeleton()
	{
		skeletonBones = GetAllChildrenInGO(base.gameObject);
		List<Rigidbody> rigidBones = StandardRagdollSkeleton.GetComponent<RigidbodyCreatureController>().RigidBones;
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject item in skeletonBones)
		{
			if (rigidBones.FirstOrDefault((Rigidbody t) => t.name == item.name) != null)
			{
				list.Add(item);
			}
		}
		skeletonBones = list;
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
}
