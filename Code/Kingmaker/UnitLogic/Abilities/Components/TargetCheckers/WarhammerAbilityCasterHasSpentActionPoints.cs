using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("89cd4dd4295101d4fa71b72146b1ed3a")]
public class WarhammerAbilityCasterHasSpentActionPoints : BlueprintComponent, IAbilityCasterRestriction
{
	public bool not;

	public bool checkMP;

	public bool checkAP;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		PartUnitCombatState combatStateOptional = caster.GetCombatStateOptional();
		if (combatStateOptional == null)
		{
			return true;
		}
		bool result = true;
		if (checkMP && combatStateOptional.ActionPointsBlue == (float)(int)combatStateOptional.WarhammerInitialAPBlue != not)
		{
			result = false;
		}
		if (checkAP && combatStateOptional.ActionPointsYellow == (int)combatStateOptional.WarhammerInitialAPYellow != not)
		{
			result = false;
		}
		return result;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.UnavailableGeneric;
	}
}
