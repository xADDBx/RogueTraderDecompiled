using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.UI.DollRoom;
using Kingmaker.Visual.Animation;
using Unity.Collections;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class Skeleton : ScriptableObject
{
	[Serializable]
	public class Bone
	{
		public string Name;

		public Vector3 Scale = Vector3.one;

		public Vector3 Offset = Vector3.zero;

		public bool ApplyOffset;

		[Tooltip("Модификатор не будет применяться, если на персонаже есть хотя бы одно из этих ЕЕ")]
		public List<EquipmentEntity> IgnoreIfCharacterContainsEE;

		[NonSerialized]
		public Vector3 OriginalOffset = Vector3.zero;

		public int NameHash => Animator.StringToHash(Name);

		public Transform Transform { get; set; }

		public Rigidbody Rigidbody { get; set; }

		public bool NeedsReapply()
		{
			if (!(Scale != Vector3.one))
			{
				return Offset != Vector3.zero;
			}
			return true;
		}
	}

	public struct BoneData
	{
		public Vector3 Scale;

		public Vector3 Offset;

		public bool ApplyOffset;
	}

	private bool m_IsDirty;

	public CharacterFxBonesMap CharacterFxBonesMap;

	[Tooltip("Required only for copying list of bones in list below")]
	public GameObject RaceBoneHierarchyObject;

	public AnimationSet AnimationSetOverride;

	public List<Bone> Bones;

	[SerializeField]
	private DollRoomCameraZoomPreset m_DollRoomZoomPreset;

	[CanBeNull]
	private Dictionary<string, Bone> m_BonesByName;

	private NativeArray<BoneData> m_BoneDataForJob;

	public DollRoomCameraZoomPreset DollRoomZoomPreset => m_DollRoomZoomPreset;

	public Dictionary<string, Bone> BonesByName
	{
		get
		{
			if (m_BonesByName == null)
			{
				m_BonesByName = new Dictionary<string, Bone>();
				foreach (Bone bone in Bones)
				{
					m_BonesByName[bone.Name] = bone;
				}
			}
			return m_BonesByName;
		}
	}

	private void CreateBoneData()
	{
		if (m_BoneDataForJob.IsCreated)
		{
			m_BoneDataForJob.Dispose();
		}
		m_BoneDataForJob = new NativeArray<BoneData>(Bones.Count, Allocator.Persistent);
		for (int i = 0; i < Bones.Count; i++)
		{
			m_BoneDataForJob[i] = new BoneData
			{
				ApplyOffset = Bones[i].ApplyOffset,
				Offset = Bones[i].Offset,
				Scale = Bones[i].Scale
			};
		}
	}

	public NativeArray<BoneData> GetBoneData()
	{
		if (!m_BoneDataForJob.IsCreated)
		{
			CreateBoneData();
		}
		return m_BoneDataForJob;
	}

	public void SortBonesAlphabetically()
	{
		Bones.Sort(delegate(Bone x, Bone y)
		{
			if (x.Name == null && y.Name == null)
			{
				return 0;
			}
			if (x.Name == null)
			{
				return -1;
			}
			return (y.Name == null) ? 1 : x.Name.CompareTo(y.Name);
		});
	}

	public void SetSkeletonDirty()
	{
		m_IsDirty = true;
	}

	public bool IsDirty()
	{
		return m_IsDirty;
	}

	public void ResetDirty()
	{
		m_IsDirty = false;
	}

	private void OnDisable()
	{
		if (m_BoneDataForJob.IsCreated)
		{
			m_BoneDataForJob.Dispose();
		}
	}
}
