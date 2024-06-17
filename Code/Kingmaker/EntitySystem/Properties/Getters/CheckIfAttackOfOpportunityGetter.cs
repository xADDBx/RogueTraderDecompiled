using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("5ffd55f3680349e0b0dda12f892c7a12")]
public class CheckIfAttackOfOpportunityGetter : PropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		AbilityData abilityData = base.PropertyContext.Ability ?? this.GetRule().Reason.Ability;
		if ((object)abilityData != null && abilityData.IsAttackOfOpportunity)
		{
			return 1;
		}
		return 0;
	}

	protected override string GetInnerCaption()
	{
		return "Check if Attack of Opportunity";
	}
}
