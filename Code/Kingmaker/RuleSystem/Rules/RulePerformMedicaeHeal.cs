using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformMedicaeHeal : RulebookTargetEvent
{
	public readonly ValueModifiersManager HealValueModifiers = new ValueModifiersManager();

	public readonly int BaseHeal;

	public readonly RulePerformSkillCheck MedicaeCheckRule;

	public readonly PartHealth TargetHealth;

	public bool AllowWoundsHealing { get; set; }

	public bool OutOfCombatHeal { get; private set; }

	public int ResultHealValue { get; private set; }

	public int ResultHitPointsReductionValue { get; private set; }

	[CanBeNull]
	public RuleHealDamage ResultHealDamageRule { get; private set; }

	public RulePerformMedicaeHeal([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, int baseHeal)
		: base(initiator, target)
	{
		TargetHealth = Target.GetHealthOptional();
		BaseHeal = baseHeal;
		MedicaeCheckRule = new RulePerformSkillCheck(base.ConcreteInitiator, StatType.SkillMedicae, 0);
	}

	public RulePerformMedicaeHeal([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, int baseHeal)
		: this((MechanicEntity)initiator, (MechanicEntity)target, baseHeal)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (TargetHealth == null || TargetHealth.Damage < 1 || base.ConcreteTarget.IsDead)
		{
			return;
		}
		OutOfCombatHeal = !Game.Instance.Player.IsInCombat;
		Rulebook.Trigger(MedicaeCheckRule);
		if (!OutOfCombatHeal)
		{
			if (!MedicaeCheckRule.ResultIsSuccess)
			{
				return;
			}
			HealValueModifiers.Add(MedicaeCheckRule.ResultDegreeOfSuccess, this, ModifierDescriptor.DegreeOfSuccess);
		}
		ResultHealValue = (OutOfCombatHeal ? TargetHealth.Damage : (BaseHeal + HealValueModifiers.Value));
		ResultHealValue = Math.Min(TargetHealth.Damage, ResultHealValue + ResultHitPointsReductionValue);
		RuleHealDamage ruleHealDamage = new RuleHealDamage(base.ConcreteInitiator, base.ConcreteTarget, ResultHealValue);
		ResultHealDamageRule = Rulebook.Trigger(ruleHealDamage);
		ResultHealValue = ruleHealDamage.Value;
		if (AllowWoundsHealing)
		{
			if (TargetHealth.WoundFreshStacks > 0)
			{
				TargetHealth.HealFreshWound(1);
			}
			else if (MedicaeCheckRule.ResultIsSuccess)
			{
				TargetHealth.HealOldWound(1);
			}
		}
		EventBus.RaiseEvent(delegate(IMedicaeHealingHandler h)
		{
			h.HandleMedicaeHealing(this);
		});
	}
}
