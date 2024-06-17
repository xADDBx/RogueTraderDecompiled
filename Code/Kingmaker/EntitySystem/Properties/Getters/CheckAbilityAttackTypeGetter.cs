using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("76fa6ff8c9162f9489df4de07a55750c")]
public class CheckAbilityAttackTypeGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public AttackAbilityType Type;

	protected override string GetInnerCaption()
	{
		return $"Ability AttackType is {Type}";
	}

	protected override int GetBaseValue()
	{
		AbilityData ability = this.GetAbility();
		if ((object)ability == null)
		{
			return 0;
		}
		if (Type == ability.Blueprint.AttackType)
		{
			return 1;
		}
		if (Type == AttackAbilityType.Pattern && ability.Blueprint.AttackType != AttackAbilityType.Scatter && ability.Blueprint.PatternSettings != null)
		{
			return 1;
		}
		return 0;
	}
}
