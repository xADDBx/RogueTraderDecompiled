using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking.Serialization;
using Kingmaker.Visual.CharactersRigidbody;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class PartSavedRagdollState : EntityPart, IHashable
{
	[JsonProperty]
	[GameStateIgnore]
	private List<RigidbodyCreatureController.BoneData> m_BoneData = new List<RigidbodyCreatureController.BoneData>();

	[JsonProperty]
	private bool m_Active;

	public bool Active => m_Active;

	public void SaveRagdollState(RigidbodyCreatureController controller)
	{
		m_Active = controller.IsActive;
		if (m_Active)
		{
			controller.SaveBonesPosition(m_BoneData);
		}
		else
		{
			m_BoneData.Clear();
		}
	}

	public void RestoreRagdollState(RigidbodyCreatureController controller)
	{
		if (m_Active)
		{
			controller.RagdollCurrentPositions = m_BoneData;
			controller.RestoreRagdollPositions();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Active);
		return result;
	}
}
