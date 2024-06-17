using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("ce32e7ec12947bc40939f3f83057d307")]
public class WarhammerAbilityIsUsingMelee : BlueprintComponent, IAbilityRestriction
{
	public string GetAbilityRestrictionUIText()
	{
		return "Must use melee weapon";
	}

	public bool IsAbilityRestrictionPassed(AbilityData ability)
	{
		return ability.Weapon?.Blueprint.IsMelee ?? false;
	}
}
