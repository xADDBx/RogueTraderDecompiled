using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("1115bf7464c8a8242aedc07680b8705c")]
public class DodgePenetrationGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired
{
	public PropertyTargetType Target;

	public bool NoTarget;

	protected override int GetBaseValue()
	{
		if (NoTarget)
		{
			return Rulebook.Trigger(new RuleCalculateDodgePenetration((UnitEntity)base.CurrentEntity)).ResultDodgePenetration;
		}
		if (!(this.GetTargetByType(Target) is UnitEntity defender))
		{
			return 0;
		}
		RuleCalculateDodgeChance ruleCalculateDodgeChance = new RuleCalculateDodgeChance(defender, base.CurrentEntity, this.GetAbility());
		Rulebook.Trigger(ruleCalculateDodgeChance);
		int num = 0;
		foreach (Modifier item in ruleCalculateDodgeChance.DodgeValueModifiers.List)
		{
			if (item.Value <= 0)
			{
				num -= item.Value;
			}
		}
		return num;
	}

	protected override string GetInnerCaption()
	{
		return "Dodge Penetration";
	}
}
