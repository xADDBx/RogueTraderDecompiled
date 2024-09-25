using System.Collections.Generic;
using System.Linq;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.CharactersRigidbody;

public class Ragdoll : MonoBehaviour
{
	public GameObject RagdollPrefab;

	public bool FromBakedCharacter;

	public GameObject RagdollRoot;

	[SerializeField]
	private RigidbodyCreatureController m_GameRigidbodyCreatureController;

	[SerializeField]
	private bool m_RaceRagdollOptionsCopied = true;

	[SerializeField]
	private GameObject m_SkeletonOverride;

	private List<RagdollBone> m_RagdollBones = new List<RagdollBone>();

	private bool m_MirrorHappened;

	public void Start()
	{
		if (FromBakedCharacter && RagdollPrefab != null)
		{
			PFLog.Default.Warning("FromBakedCharacter = true. Need not prefab. Prefab deleted..." + base.gameObject.name);
			RagdollPrefab = null;
		}
		else
		{
			GetComponentsInChildren(m_RagdollBones);
		}
	}

	public void PostEventWithSurfaceAdding()
	{
		GameObject gameObject = base.transform.parent.GetComponentsInChildren<Transform>(includeInactive: true).FirstOrDefault((Transform t) => t.name == "Ragdoll_Pelvis").gameObject;
		GameObject obj = base.transform.parent.GetComponentsInChildren<Transform>(includeInactive: true).FirstOrDefault((Transform t) => t.name == "Ragdoll_Spine_3").gameObject;
		RagdollPostEventWithSurface ragdollPostEventWithSurface = gameObject.AddComponent<RagdollPostEventWithSurface>();
		RagdollPostEventWithSurface ragdollPostEventWithSurface2 = obj.AddComponent<RagdollPostEventWithSurface>();
		ragdollPostEventWithSurface.GetInfoAboutCharacter();
		ragdollPostEventWithSurface2.GetInfoAboutCharacter();
		List<RagdollPostEventWithSurface> list = new List<RagdollPostEventWithSurface>();
		list.Add(ragdollPostEventWithSurface);
		list.Add(ragdollPostEventWithSurface2);
		if (m_GameRigidbodyCreatureController == null)
		{
			m_GameRigidbodyCreatureController = base.gameObject.GetComponent<RigidbodyCreatureController>();
		}
		m_GameRigidbodyCreatureController.EventTargets = list;
	}

	public void MirrorRagdollForLeftHanded()
	{
		if (!m_MirrorHappened && base.transform.localScale.x == -1f)
		{
			UnParentRagdollForLeftHanded();
			SwapLeftAndRightForLeftHanded();
			ReRotateBecauseOfMirror();
			m_MirrorHappened = true;
		}
	}

	public void LinkSkeletons(Transform skin, Transform ragdoll)
	{
		ragdoll.name = "Ragdoll_" + ragdoll.name;
		RagdollBone ragdollBone = skin.gameObject.AddComponent<RagdollBone>();
		ragdollBone.BoneInRagdollSkeleton = ragdoll;
		ragdollBone.BoneInRagdollSkeleton.gameObject.AddComponent<LinkToGetPose>().Link = ragdollBone.transform;
		if (ragdoll.GetComponent<Rigidbody>() != null)
		{
			ragdollBone.RigidbodyBone = true;
		}
		foreach (Transform child in skin.Children())
		{
			if (!(child.name == "L_Neck_Muscl") && !(child.name == "R_Neck_Muscl") && !(child.name == "Stomach") && !(child.name == "L_A") && !(child.name == "R_A") && !(child.name == "L_B") && !(child.name == "R_B") && child.name.IndexOf("_toe") == -1 && child.name.IndexOf("_Toe") == -1 && child.name.IndexOf("ADJ") == -1)
			{
				Transform transform = ragdoll.Children().FirstOrDefault((Transform c) => c.name == child.name);
				if (transform != null)
				{
					LinkSkeletons(child, transform);
				}
			}
		}
	}

	public void Update()
	{
		if (FromBakedCharacter || !(RagdollPrefab == null))
		{
			if (!m_RaceRagdollOptionsCopied)
			{
				GetComponentsInChildren(m_RagdollBones);
			}
			MirrorRagdollForLeftHanded();
			CopyPoseFromRagdoll();
		}
	}

	public void CopyPoseFromRagdoll(bool force = false)
	{
		if (!m_GameRigidbodyCreatureController.RagdollWorking && !force)
		{
			return;
		}
		if (m_RagdollBones.Count == 0)
		{
			GetComponentsInChildren(m_RagdollBones);
		}
		foreach (RagdollBone ragdollBone in m_RagdollBones)
		{
			if (ragdollBone.RigidbodyBone)
			{
				ragdollBone.transform.position = ragdollBone.BoneInRagdollSkeleton.transform.position;
				ragdollBone.transform.rotation = ragdollBone.BoneInRagdollSkeleton.transform.rotation;
			}
		}
	}

	public void CopyPoseToRagdollSkeleton()
	{
		foreach (RagdollBone ragdollBone in m_RagdollBones)
		{
			LinkToGetPose component = ragdollBone.BoneInRagdollSkeleton.GetComponent<LinkToGetPose>();
			if (component != null)
			{
				ragdollBone.BoneInRagdollSkeleton.position = component.Link.position;
				ragdollBone.BoneInRagdollSkeleton.rotation = component.Link.rotation;
			}
			else
			{
				component = ragdollBone.BoneInRagdollSkeleton.parent.GetComponent<LinkToGetPose>();
				ragdollBone.BoneInRagdollSkeleton.parent.position = component.Link.position;
				ragdollBone.BoneInRagdollSkeleton.parent.rotation = component.Link.rotation;
			}
		}
	}

	public void UnParentRagdollForLeftHanded()
	{
		RagdollRoot.transform.parent = GetComponentInParent<Character>().transform;
		RagdollRoot.transform.localPosition = new Vector3(0f, 0f, 0f);
		RagdollRoot.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, -90f));
		RagdollRoot.transform.localScale = new Vector3(1f, 1f, 1f);
	}

	public GameObject SonForReRotateBone(Transform bone, string prefix)
	{
		Vector3 euler = ((bone.name == prefix + "Pelvis") ? new Vector3(0f, 0f, 0f) : ((bone.name == prefix + "L_Clavicle") ? new Vector3(0f, 180f, 180f) : ((bone.name == prefix + "R_Clavicle") ? new Vector3(0f, 180f, 180f) : ((!(bone.name == prefix + "Spine_1") && !(bone.name == prefix + "Spine_2") && !(bone.name == prefix + "Spine_3") && !(bone.name == prefix + "Neck") && !(bone.name == prefix + "Head")) ? new Vector3(0f, 180f, 180f) : new Vector3(0f, 180f, 0f)))));
		GameObject obj = new GameObject();
		obj.transform.parent = bone;
		obj.transform.localPosition = new Vector3(0f, 0f, 0f);
		obj.transform.localScale = new Vector3(1f, 1f, 1f);
		obj.transform.localRotation = Quaternion.Euler(euler);
		obj.name = "ForMirror_" + bone.name;
		return obj;
	}

	public void ReRotateBecauseOfMirror()
	{
		foreach (RagdollBone ragdollBone in m_RagdollBones)
		{
			GameObject gameObject = SonForReRotateBone(ragdollBone.transform, "");
			GameObject gameObject2 = SonForReRotateBone(ragdollBone.BoneInRagdollSkeleton, "Ragdoll_");
			ragdollBone.BoneInRagdollSkeleton.transform.GetComponent<LinkToGetPose>().Link = gameObject.transform;
			ragdollBone.BoneInRagdollSkeleton = gameObject2.transform;
		}
	}

	public void SwapLeftAndRightForLeftHanded()
	{
		List<GameObject> list = new List<GameObject>();
		list.Add(base.gameObject.transform.GetComponentsInChildren<Transform>().FirstOrDefault((Transform b) => b.name == "L_Pre_Up_Leg").gameObject);
		list.Add(base.gameObject.transform.GetComponentsInChildren<Transform>().FirstOrDefault((Transform b) => b.name == "R_Pre_Up_Leg").gameObject);
		list.Add(base.gameObject.transform.GetComponentsInChildren<Transform>().FirstOrDefault((Transform b) => b.name == "L_Clavicle").gameObject);
		list.Add(base.gameObject.transform.GetComponentsInChildren<Transform>().FirstOrDefault((Transform b) => b.name == "R_Clavicle").gameObject);
		SwapLandR(list[0], list[1]);
		SwapLandR(list[2], list[3]);
	}

	public void SwapLandR(GameObject go1, GameObject go2)
	{
		RagdollBone[] componentsInChildren = go1.GetComponentsInChildren<RagdollBone>();
		foreach (RagdollBone ragdollBone in componentsInChildren)
		{
			string search = "";
			if (ragdollBone.BoneInRagdollSkeleton.name.IndexOf("Ragdoll_L_") != -1)
			{
				search = "R" + ragdollBone.BoneInRagdollSkeleton.name.Substring(9);
			}
			else if (ragdollBone.BoneInRagdollSkeleton.name.IndexOf("Ragdoll_R_") != -1)
			{
				search = "L" + ragdollBone.BoneInRagdollSkeleton.name.Substring(9);
			}
			RagdollBone ragdollBone2 = go2.GetComponentsInChildren<RagdollBone>().FirstOrDefault((RagdollBone b) => b.name == search);
			Transform boneInRagdollSkeleton = ragdollBone.BoneInRagdollSkeleton;
			ragdollBone.BoneInRagdollSkeleton = ragdollBone2.BoneInRagdollSkeleton;
			ragdollBone2.BoneInRagdollSkeleton = boneInRagdollSkeleton;
			ragdollBone.BoneInRagdollSkeleton.transform.GetComponent<LinkToGetPose>().Link = ragdollBone.transform;
			ragdollBone2.BoneInRagdollSkeleton.transform.GetComponent<LinkToGetPose>().Link = ragdollBone2.transform;
		}
	}
}
