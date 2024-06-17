using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Networking.Serialization;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment;

public class SavedDismembermentState : BaseUnitPart, IHashable
{
	[JsonProperty]
	[CanBeNull]
	[GameStateIgnore]
	private List<BoneTransformData> m_Bones;

	[JsonProperty]
	[GameStateIgnore]
	public int SetIndex = -1;

	[JsonProperty]
	[GameStateIgnore]
	public int DestroyedPieceIndex = -1;

	[JsonProperty]
	private Dictionary<string, UnitDismembermentManager.BoneDataObsolete> Bones
	{
		set
		{
			m_Bones = value.Values.Select((UnitDismembermentManager.BoneDataObsolete i) => new BoneTransformData(i.Position, i.Rotation)).ToList();
		}
	}

	public bool Active => SetIndex >= 0;

	private static void ForEachBone(UnitDismembermentManager manager, Action<Transform> action)
	{
		ForEachBoneInternal(manager.GetComponentInChildren<DismembermentSetBehaviour>().transform, action);
		static void ForEachBoneInternal(Transform t, Action<Transform> a)
		{
			a(t);
			for (int i = 0; i < t.childCount; i++)
			{
				ForEachBoneInternal(t.GetChild(i), a);
			}
		}
	}

	public void SaveDismembermentState(UnitDismembermentManager dismembermentManager)
	{
		if (dismembermentManager.Dismembered)
		{
			SetIndex = dismembermentManager.SetIndex;
			DestroyedPieceIndex = dismembermentManager.DestroyedPieceIndex;
			m_Bones = new List<BoneTransformData>();
			ForEachBone(dismembermentManager, delegate(Transform i)
			{
				m_Bones.Add(new BoneTransformData(i.localPosition, i.localRotation));
			});
		}
		else
		{
			SetIndex = -1;
			DestroyedPieceIndex = -1;
			m_Bones = null;
		}
	}

	public void RestoreDismembermentState(UnitDismembermentManager dismembermentManager)
	{
		if (SetIndex < 0 || m_Bones == null)
		{
			return;
		}
		dismembermentManager.SetIndex = SetIndex;
		dismembermentManager.DestroyedPieceIndex = DestroyedPieceIndex;
		dismembermentManager.RestoreState();
		int index = 0;
		ForEachBone(dismembermentManager, delegate(Transform i)
		{
			if (index < m_Bones.Count)
			{
				i.localPosition = m_Bones[index].Position;
				i.localRotation = m_Bones[index].Rotation;
				index++;
			}
		});
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
