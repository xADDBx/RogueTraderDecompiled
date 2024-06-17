using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class RuleCalculateHeal : RulebookTargetEvent
{
	[CanBeNull]
	public readonly PartHealth TargetHealth;

	[CanBeNull]
	public RuleRollD100 UICriticalBonusRoll;

	[CanBeNull]
	public int? UICriticalBonusChance;

	public int UIPercentCriticalBonus;

	public readonly List<Tuple<int, string>> UIPercentCriticalBonuses = new List<Tuple<int, string>>();

	public DiceFormula HealFormula { get; private set; }

	public int DiceResult { get; private set; }

	public int ValueWithoutReduction { get; private set; }

	public int Value { get; private set; }

	public int PercentBonus { get; set; }

	public int FlatBonus { get; set; }

	public int Bonus { get; }

	public bool UIIsCritical
	{
		get
		{
			if (UICriticalBonusRoll != null && UICriticalBonusChance.HasValue)
			{
				return UICriticalBonusRoll.Result <= UICriticalBonusChance;
			}
			return false;
		}
	}

	public int UIValueBase { get; private set; }

	public int UIValue => Value;

	public int UIValueWithoutCriticalBonus { get; private set; }

	public RuleCalculateHeal([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, DiceFormula dice, int bonus)
		: this((MechanicEntity)initiator, (MechanicEntity)target, dice, bonus)
	{
	}

	public RuleCalculateHeal([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, DiceFormula dice, int bonus)
		: base(initiator, target)
	{
		TargetHealth = target.GetHealthOptional();
		HealFormula = dice;
		Bonus = bonus;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (TargetHealth != null && !this.SkipBecauseOfShadow())
		{
			HealFormula = new DiceFormula(HealFormula.Rolls, HealFormula.Dice);
			DiceResult = Dice.D(HealFormula);
			int val = DiceResult + Bonus;
			ValueWithoutReduction = Math.Max(0, val);
			UIValueBase = Math.Min(ValueWithoutReduction, TargetHealth.Damage);
			UIValueWithoutCriticalBonus = (UIValueBase + FlatBonus) * (100 + PercentBonus - UIPercentCriticalBonus) / 100;
			Value = (Math.Min(ValueWithoutReduction, TargetHealth.Damage) + FlatBonus) * (100 + PercentBonus) / 100;
		}
	}
}
