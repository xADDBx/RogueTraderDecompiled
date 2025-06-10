using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("2e8f32b9e3b14de1a7c597fba0b0fd47")]
public class AbilityCasterCustomRestriction : BlueprintComponent, IAbilityCasterRestriction
{
	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		return Restriction.IsPassed(new PropertyContext(caster, null));
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}
}
