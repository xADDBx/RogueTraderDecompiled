using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("e0845d07164f64443b3764e12edfbeb3")]
public class AbilityAllowAdditionalTargetTypes : AllowTargetingTypeComponent
{
	public override bool IsRestrictionPassed(AbilityData abilityData)
	{
		PropertyContext context = new PropertyContext(abilityData);
		return m_Restrictions.IsPassed(context);
	}
}
