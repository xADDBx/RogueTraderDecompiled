using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("f36866d65515f6d47be143a981139a42")]
public class DodgeChanceGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional
{
	public PropertyTargetType Attacker;

	public bool NoTarget;

	public bool OnlyNegativeModifiers;

	public bool DoNotCountPerception;

	protected override int GetBaseValue()
	{
		if (NoTarget)
		{
			if (!(base.CurrentEntity is UnitEntity defender))
			{
				return 0;
			}
			return Rulebook.Trigger(new RuleCalculateDodgeChance(defender)
			{
				HasNoTarget = NoTarget
			}).UncappedResult;
		}
		if (!(base.CurrentEntity is UnitEntity defender2))
		{
			return 0;
		}
		if (!(this.GetTargetByType(Attacker) is UnitEntity attacker))
		{
			return 0;
		}
		RuleCalculateDodgeChance ruleCalculateDodgeChance = new RuleCalculateDodgeChance(defender2, attacker, this.GetAbility())
		{
			HasNoTarget = NoTarget
		};
		Rulebook.Trigger(ruleCalculateDodgeChance);
		int num = 0;
		if (OnlyNegativeModifiers)
		{
			foreach (Modifier item in ruleCalculateDodgeChance.DodgeValueModifiers.List)
			{
				if (item.Value <= 0 && (!DoNotCountPerception || (item.Descriptor != ModifierDescriptor.AttackerPerception && item.Descriptor != ModifierDescriptor.AttackerAgility)))
				{
					num += item.Value;
				}
			}
		}
		else
		{
			num = ruleCalculateDodgeChance.UncappedResult;
		}
		return num;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (NoTarget)
		{
			return "Dodge of " + FormulaTargetScope.Current + " against abstract attack";
		}
		return "Dodge of " + FormulaTargetScope.Current + " against " + Attacker.Colorized();
	}
}
