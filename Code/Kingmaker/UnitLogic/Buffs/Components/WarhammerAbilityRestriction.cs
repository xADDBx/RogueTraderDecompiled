using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("4154c6f5c7b64b23a2102278fc83645b")]
public class WarhammerAbilityRestriction : UnitBuffComponentDelegate, IHashable
{
	[SerializeField]
	private bool m_UseAbilityGroups;

	[SerializeField]
	private bool m_UseOnlyListedAbilities;

	[SerializeField]
	[HideIf("m_UseAbilityGroups")]
	private List<BlueprintAbilityReference> m_Abilities;

	[SerializeField]
	[ShowIf("m_UseAbilityGroups")]
	private List<BlueprintAbilityGroupReference> m_AbilityGroups;

	[SerializeField]
	[Space(4f)]
	[ShowIf("m_UseAbilityGroups")]
	private bool m_UltimateAbilities;

	public bool AbilityIsRestricted(AbilityData abilityData)
	{
		bool flag = ((!m_UseAbilityGroups) ? m_Abilities.Any((BlueprintAbilityReference reference) => reference.GetBlueprint() == abilityData.Blueprint) : (m_AbilityGroups.Any((BlueprintAbilityGroupReference group) => abilityData.AbilityGroups.Contains(group)) || (m_UltimateAbilities && abilityData.IsUltimate)));
		if (!m_UseOnlyListedAbilities)
		{
			return flag;
		}
		return !flag;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
