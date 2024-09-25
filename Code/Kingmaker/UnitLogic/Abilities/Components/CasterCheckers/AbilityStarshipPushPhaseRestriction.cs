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
[TypeId("16478eab25dfeb64b8375927b3e8f08b")]
public class AbilityStarshipPushPhaseRestriction : BlueprintComponent, IAbilityCasterRestriction
{
	public bool allowMiddlePhase;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		if (!(caster is StarshipEntity starshipEntity))
		{
			return false;
		}
		if (!starshipEntity.Navigation.IsAccelerationMovementPhase)
		{
			if (allowMiddlePhase)
			{
				return !starshipEntity.Navigation.IsEndingMovementPhase;
			}
			return false;
		}
		return true;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		if (allowMiddlePhase)
		{
			StarshipEntity obj = caster as StarshipEntity;
			if (obj == null || obj.Navigation.IsEndingMovementPhase)
			{
				return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
			}
		}
		return LocalizedTexts.Instance.Reasons.PastPushPhase;
	}
}
