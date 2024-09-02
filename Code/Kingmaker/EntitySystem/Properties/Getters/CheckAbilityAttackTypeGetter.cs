using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("76fa6ff8c9162f9489df4de07a55750c")]
public class CheckAbilityAttackTypeGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public AttackAbilityType Type;

	protected override string GetInnerCaption(bool useLineBreaks)
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
		switch (Type)
		{
		case AttackAbilityType.Melee:
			if (!ability.IsMelee)
			{
				return 0;
			}
			return 1;
		case AttackAbilityType.Scatter:
			if (!ability.IsScatter)
			{
				return 0;
			}
			return 1;
		case AttackAbilityType.Pattern:
		{
			bool flag = ability.Blueprint.GetComponents<FakeAttackType>().Any((FakeAttackType fake) => fake.CountAsAoE && fake.CountAsScatter) || (bool)ability.Caster.Features.AllAttacksCountAsAoe;
			if (!((ability.IsAOE && !ability.IsScatter) || flag))
			{
				return 0;
			}
			return 1;
		}
		case AttackAbilityType.SingleShot:
			if (!ability.IsSingleShot)
			{
				return 0;
			}
			return 1;
		default:
			return 0;
		}
	}
}
