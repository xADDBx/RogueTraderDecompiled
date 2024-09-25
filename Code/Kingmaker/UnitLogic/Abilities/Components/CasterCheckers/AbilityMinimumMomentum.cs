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
[TypeId("8f37b041df775b442b00e537f5555afa")]
public class AbilityMinimumMomentum : BlueprintComponent, IAbilityCasterRestriction
{
	public int MinimumMomentum;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		return Game.Instance.TurnController.MomentumController.GetGroup(caster)?.Momentum >= MinimumMomentum;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.NotYetHeroicAct;
	}
}
