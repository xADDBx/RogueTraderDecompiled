using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("4de439526c0bd2a4893745484ca53d77")]
public class CheckAbilityAPCostGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		return this.GetAbility().CalculateActionPointCost();
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability is Charge";
	}
}
