using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class RuleHealStatDamage : RulebookTargetEvent<UnitEntity>
{
	public readonly StatType StatType;

	public int Amount { get; }

	public bool HealAll { get; }

	public int HealedDamage { get; private set; }

	public int HealedDrain { get; private set; }

	public bool ShouldHealDrain { get; set; }

	private RuleHealStatDamage([NotNull] MechanicEntity initiator, [NotNull] UnitEntity target, StatType stat, int amount, bool healAll)
		: base(initiator, target)
	{
		if (target.Stats.GetStat<ModifiableValueAttributeStat>(stat) == null)
		{
			PFLog.Default.Error("Stat '{0}' is not a ModifiableValueAttributeStat", stat);
			return;
		}
		StatType = stat;
		HealAll = healAll;
		if (!HealAll)
		{
			Amount = amount;
			if (Amount < 1)
			{
				PFLog.Default.Error("Invalid heal value (stat '{0}'): {1}", stat, Amount);
			}
		}
	}

	private RuleHealStatDamage([NotNull] IMechanicEntity initiator, [NotNull] UnitEntity target, StatType stat, int amount, bool healAll)
		: this((MechanicEntity)initiator, target, stat, amount, healAll)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (!this.SkipBecauseOfShadow())
		{
			DoHeal(base.Target);
		}
	}

	private void DoHeal(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			return;
		}
		ModifiableValueAttributeStat stat = unit.Stats.GetStat<ModifiableValueAttributeStat>(StatType);
		if (HealAll)
		{
			HealedDamage = stat.Damage;
			HealedDrain = stat.Drain;
			stat.Damage = 0;
			stat.Drain = 0;
		}
		else if (Amount > 0)
		{
			if (ShouldHealDrain)
			{
				HealedDrain = Math.Min(stat.Drain, Amount);
				stat.Drain -= HealedDrain;
			}
			else
			{
				HealedDamage = Math.Min(stat.Damage, Amount);
				stat.Damage -= HealedDamage;
			}
		}
	}
}
