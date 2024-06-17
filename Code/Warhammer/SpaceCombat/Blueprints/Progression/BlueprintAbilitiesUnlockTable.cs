using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints.Progression;

[TypeId("c39f1caeb4a4fd74d8531aa57d797fa0")]
public class BlueprintAbilitiesUnlockTable : BlueprintScriptableObject
{
	[Serializable]
	private struct AbilityList
	{
		public BlueprintAbilityReference[] Abilities;
	}

	[SerializeField]
	private AbilityList[] m_UnlockTable;

	private Dictionary<BlueprintAbility, int> m_UnlockCache;

	public bool IsUnlocked(BlueprintAbility ability, int level)
	{
		int unlockLevel = GetUnlockLevel(ability);
		return level >= unlockLevel;
	}

	public int GetUnlockLevel(BlueprintAbility ability)
	{
		if (m_UnlockCache == null)
		{
			CreateCache();
		}
		if (m_UnlockCache.TryGetValue(ability, out var value))
		{
			return value;
		}
		return 0;
	}

	private void CreateCache()
	{
		m_UnlockCache = new Dictionary<BlueprintAbility, int>();
		for (int i = 0; i < m_UnlockTable.Length; i++)
		{
			BlueprintAbilityReference[] abilities = m_UnlockTable[i].Abilities;
			for (int j = 0; j < abilities.Length; j++)
			{
				BlueprintAbility blueprintAbility = abilities[j]?.Get();
				if (blueprintAbility != null)
				{
					m_UnlockCache[blueprintAbility] = i;
				}
			}
		}
	}
}
