using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("d92b5c4b3f6b49ef9e138e8b4ae2b7aa")]
public class CheckAbilityBaseActionPointCostGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public int Cost;

	protected override int GetBaseValue()
	{
		if (this.GetAbility()?.Blueprint.ActionPointCost != Cost)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Ability costs {Cost} action points";
	}
}
