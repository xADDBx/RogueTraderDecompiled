using System;
using JetBrains.Annotations;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class RuleDealStatDamage : RulebookTargetEvent<UnitEntity>
{
	public readonly ModifiableValueAttributeStat Stat;

	public readonly DiceFormula Dices;

	public readonly int Bonus;

	public int TotalBonus { get; private set; }

	public int Result { get; private set; }

	public bool IsDrain { get; set; }

	public bool Maximize { get; set; }

	public bool Empower { get; set; }

	public DamageCriticalModifierType? CriticalModifier { get; set; }

	public bool HalfBecauseSavingThrow { get; set; }

	public bool Immune { get; set; }

	public int? MinStatScoreAfterDamage { get; set; }

	public RuleDealStatDamage([NotNull] MechanicEntity initiator, [NotNull] UnitEntity target, StatType stat, DiceFormula dices, int bonus)
		: base(initiator, target)
	{
		Stat = target.Stats.GetStat<ModifiableValueAttributeStat>(stat);
		if (Stat == null)
		{
			PFLog.Default.Error("Stat '{0}' is not a ModifiableValueAttributeStat", stat);
		}
		else
		{
			Dices = dices;
			Bonus = bonus;
		}
	}

	public RuleDealStatDamage([NotNull] IMechanicEntity initiator, [NotNull] UnitEntity target, StatType stat, DiceFormula dices, int bonus)
		: this((MechanicEntity)initiator, target, stat, dices, bonus)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (!this.SkipBecauseOfShadow() && !Immune && !AbstractUnitCommand.CommandTargetUntargetable((MechanicEntity)base.Initiator, base.Target, this) && Stat != null && Stat.Enabled && !((MechanicEntity)base.Initiator).IsAttackingGreenNPC(base.Target))
		{
			int num = RollDamage(Dices, CriticalModifier?.IntValue() ?? 1, Maximize);
			float num2 = (float)(num + Bonus) * (Empower ? 1.5f : 1f) * (HalfBecauseSavingThrow ? 0.5f : 1f);
			int val = (MinStatScoreAfterDamage.HasValue ? (Stat.ModifiedValue - MinStatScoreAfterDamage.Value) : int.MaxValue);
			Result = Math.Min(val, (int)num2);
			TotalBonus = Result - num;
			if (IsDrain)
			{
				Stat.Drain += Result;
			}
			else
			{
				Stat.Damage += Result;
			}
			if (Stat.ModifiedValueRaw < 1)
			{
				base.Target.LifeState.MarkedForDeath = true;
			}
		}
	}

	private static int RollDamage(DiceFormula damageFormula, int criticalModifier, bool maximized)
	{
		if (damageFormula == DiceFormula.Zero)
		{
			return 0;
		}
		if (damageFormula == DiceFormula.One)
		{
			return 1;
		}
		if (maximized)
		{
			return damageFormula.Rolls * damageFormula.Dice.Sides() * criticalModifier;
		}
		int num = 0;
		while (criticalModifier-- > 0)
		{
			num += Dice.D(damageFormula);
		}
		return num;
	}
}
