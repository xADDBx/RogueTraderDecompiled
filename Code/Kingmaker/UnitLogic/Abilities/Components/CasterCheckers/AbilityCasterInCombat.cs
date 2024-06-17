using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("901c0e3dc67f4db4597643cde3b7048a")]
public class AbilityCasterInCombat : BlueprintComponent, IAbilityCasterRestriction
{
	public bool Not;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		bool isInCombat = caster.IsInCombat;
		if (!Not)
		{
			return isInCombat;
		}
		return !isInCombat;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.CombatRequired;
	}
}
