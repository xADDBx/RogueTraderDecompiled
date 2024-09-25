using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("2d1b8d64a28167e4499598fa3808b19f")]
public class CheckAbilityIsHeroicGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public bool DesperateMeasureCounts;

	protected override int GetBaseValue()
	{
		AbilityData ability = this.GetAbility();
		if ((object)ability == null || !ability.IsHeroicAct)
		{
			if (DesperateMeasureCounts)
			{
				AbilityData ability2 = this.GetAbility();
				if ((object)ability2 != null && ability2.IsDesperateMeasure)
				{
					goto IL_0032;
				}
			}
			return 0;
		}
		goto IL_0032;
		IL_0032:
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability is Heroic Act or Desperate Measure";
	}
}
