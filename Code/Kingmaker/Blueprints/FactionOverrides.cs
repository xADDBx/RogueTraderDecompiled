using System;
using Code.GameCore.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints;

[Serializable]
public class FactionOverrides
{
	[SerializeField]
	[FormerlySerializedAs("AttackFactionsToAdd")]
	private BlueprintFactionReference[] m_AttackFactionsToAdd;

	[SerializeField]
	[FormerlySerializedAs("AttackFactionsToRemove")]
	private BlueprintFactionReference[] m_AttackFactionsToRemove;

	public ReferenceArrayProxy<BlueprintFaction> AttackFactionsToAdd
	{
		get
		{
			BlueprintReference<BlueprintFaction>[] attackFactionsToAdd = m_AttackFactionsToAdd;
			return attackFactionsToAdd;
		}
	}

	public ReferenceArrayProxy<BlueprintFaction> AttackFactionsToRemove
	{
		get
		{
			BlueprintReference<BlueprintFaction>[] attackFactionsToRemove = m_AttackFactionsToRemove;
			return attackFactionsToRemove;
		}
	}
}
