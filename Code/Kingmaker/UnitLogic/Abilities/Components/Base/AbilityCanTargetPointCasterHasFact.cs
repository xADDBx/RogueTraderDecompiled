using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("d484334692c54b3aaa787b1420f7e7ca")]
public class AbilityCanTargetPointCasterHasFact : BlueprintComponent, IAbilityCanTargetPointRestriction
{
	[SerializeField]
	private BlueprintUnitFactReference m_Fact;

	public BlueprintFact Fact => m_Fact?.Get();

	public bool IsAbilityCanTargetPointRestrictionPassed(AbilityData ability)
	{
		return ability.Caster.Facts.Contains(Fact);
	}

	public string GetAbilityCanTargetPointRestrictionUIText()
	{
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}
}
