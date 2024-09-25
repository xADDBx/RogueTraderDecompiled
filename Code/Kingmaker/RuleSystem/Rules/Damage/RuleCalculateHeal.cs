using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

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

	public bool UseDiceFormula { get; private set; }

	public DiceFormula HealFormula { get; private set; }

	public int DiceResult { get; private set; }

	public int MinHealing { get; private set; }

	public int MaxHealing { get; private set; }

	public int MinHealingModified { get; private set; }

	public int MaxHealingModified { get; private set; }

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

	public RuleCalculateHeal([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, int min, int max, int bonus)
		: base(initiator, target)
	{
		TargetHealth = target.GetHealthOptional();
		MinHealing = min;
		MaxHealing = max;
		Bonus = bonus;
		UseDiceFormula = false;
	}

	public RuleCalculateHeal([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, DiceFormula dice, int bonus)
		: base(initiator, target)
	{
		TargetHealth = target.GetHealthOptional();
		HealFormula = dice;
		Bonus = bonus;
		UseDiceFormula = true;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (TargetHealth != null && !this.SkipBecauseOfShadow())
		{
			int num = 0;
			if (UseDiceFormula)
			{
				HealFormula = new DiceFormula(HealFormula.Rolls, HealFormula.Dice);
				MinHealingModified = HealFormula.MinValue(Bonus + FlatBonus) * (100 + PercentBonus) / 100;
				MaxHealingModified = HealFormula.MaxValue(Bonus + FlatBonus) * (100 + PercentBonus) / 100;
				DiceResult = Dice.D(HealFormula);
			}
			else
			{
				float num2 = Math.Clamp((float)Dice.D(new DiceFormula(1, DiceType.D100)) / 100f, 0f, 1f);
				MinHealingModified = (MinHealing + Bonus + FlatBonus) * (100 + PercentBonus) / 100;
				MaxHealingModified = (MaxHealing + Bonus + FlatBonus) * (100 + PercentBonus) / 100;
				DiceResult = MinHealing + Mathf.RoundToInt((float)(MaxHealing - MinHealing) * num2);
			}
			num += DiceResult + Bonus;
			ValueWithoutReduction = Math.Max(0, num);
			UIValueBase = Math.Min(ValueWithoutReduction, TargetHealth.Damage);
			UIValueWithoutCriticalBonus = (UIValueBase + FlatBonus) * (100 + PercentBonus - UIPercentCriticalBonus) / 100;
			Value = (Math.Min(ValueWithoutReduction, TargetHealth.Damage) + FlatBonus) * (100 + PercentBonus) / 100;
		}
	}
}
