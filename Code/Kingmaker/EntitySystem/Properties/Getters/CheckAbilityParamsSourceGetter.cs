using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("7cc04be6252f6c04aa31d33cfcd0a0da")]
public class CheckAbilityParamsSourceGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public WarhammerAbilityParamsSource ParamsStouce;

	protected override int GetBaseValue()
	{
		if (this.GetAbility()?.Blueprint.AbilityParamsSource != ParamsStouce)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Ability params source is {ParamsStouce}";
	}
}
