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
		RuleCalculateDodgeChance obj = new RuleCalculateDodgeChance(defender, base.CurrentEntity, this.GetAbility())
		{
			HasNoTarget = NoTarget
		};
		Rulebook.Trigger(obj);
		int num = 0;
		foreach (Modifier item in obj.DodgeValueModifiers.List)
		{
			if (item.Value <= 0)
			{
				num -= item.Value;
			}
		}
		return num;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (NoTarget)
		{
			return "Dodge Penetration of " + FormulaTargetScope.Current + " against abstract target";
		}
		return "Dodge Penetration of " + FormulaTargetScope.Current + " against " + Target.Colorized();
	}
}
