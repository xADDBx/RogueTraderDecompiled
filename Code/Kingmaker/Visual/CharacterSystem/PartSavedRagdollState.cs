using System;
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

	[JsonProperty]
	private RigidbodyCreatureController.RagdollState m_State;

	[JsonProperty]
	private TimeSpan m_StartTime;

	[JsonProperty]
	private TimeSpan m_StartTimeToStop;

	public bool Active => m_Active;

	public void SaveRagdollState(RigidbodyCreatureController controller)
	{
		m_Active = controller.IsActive;
		if (m_Active)
		{
			controller.SaveBonesPosition(m_BoneData);
			m_State = controller.State;
			if (m_State == RigidbodyCreatureController.RagdollState.Falling)
			{
				m_StartTime = controller.StartTime;
				m_StartTimeToStop = controller.StartTimeToStop;
			}
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
			controller.RestoreRagdollPositions(m_State, m_StartTime, m_StartTimeToStop);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Active);
		result.Append(ref m_State);
		result.Append(ref m_StartTime);
		result.Append(ref m_StartTimeToStop);
		return result;
	}
}
