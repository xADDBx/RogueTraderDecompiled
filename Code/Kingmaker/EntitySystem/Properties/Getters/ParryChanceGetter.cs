using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("5fedba6883837ed43ab25613b07a422b")]
public class ParryChanceGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional
{
	public PropertyTargetType Attacker;

	public bool NoTarget;

	public bool OnlyNegativeModifiers;

	public bool DoNotCountWeaponSkillAndAgility;

	protected override int GetBaseValue()
	{
		if (NoTarget)
		{
			return Rulebook.Trigger(new RuleCalculateParryChance((UnitEntity)base.CurrentEntity)).Result;
		}
		if (!(base.CurrentEntity is UnitEntity defender))
		{
			return 0;
		}
		RuleCalculateParryChance ruleCalculateParryChance = new RuleCalculateParryChance(defender, (UnitEntity)this.GetTargetByType(Attacker), this.GetAbility());
		Rulebook.Trigger(ruleCalculateParryChance);
		int num = 0;
		if (OnlyNegativeModifiers)
		{
			foreach (Modifier item in ruleCalculateParryChance.ParryValueModifiers.List)
			{
				if (item.Value <= 0 && (!DoNotCountWeaponSkillAndAgility || (item.Descriptor != ModifierDescriptor.WeaponSkillDifference && item.Descriptor != ModifierDescriptor.AttackerAgility)))
				{
					num += item.Value;
				}
			}
		}
		else
		{
			num = ruleCalculateParryChance.Result;
		}
		return num;
	}

	protected override string GetInnerCaption()
	{
		return "Parry";
	}
}
