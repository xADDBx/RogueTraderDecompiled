using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities.Crew;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("372438e89ee542a59b57f51a16b70240")]
public class AbilityRestrictionDistributeCrewQuarters : BlueprintComponent, IAbilityRestriction
{
	public bool IsAbilityRestrictionPassed(AbilityData ability)
	{
		if (ability?.Fact?.Owner is StarshipEntity starshipEntity)
		{
			return starshipEntity.Crew.CanDistributeQuarters();
		}
		return false;
	}

	public string GetAbilityRestrictionUIText()
	{
		return "<Crew quarters module crew must be > 0 and any module crew must be not full>";
	}
}
