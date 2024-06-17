using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class RuleHealStarshipMoraleDamage : RulebookTargetEvent<BaseUnitEntity>
{
	[CanBeNull]
	public readonly PartStarshipMorale TargetMorale;

	private int Amount;

	private bool HealAll;

	public int Result { get; private set; }

	public RuleHealStarshipMoraleDamage([NotNull] MechanicEntity initiator, [NotNull] StarshipEntity target, int amount, bool healAll)
		: base(initiator, (BaseUnitEntity)target)
	{
		TargetMorale = target.Morale;
		Amount = amount;
		HealAll = healAll;
	}

	public RuleHealStarshipMoraleDamage([NotNull] IMechanicEntity initiator, [NotNull] StarshipEntity target, int amount, bool healAll)
		: this((MechanicEntity)initiator, target, amount, healAll)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (TargetMorale != null)
		{
			if (HealAll)
			{
				Result = TargetMorale.MoraleDamage;
			}
			else
			{
				Result = Math.Min(TargetMorale.MoraleDamage, Amount);
			}
			TargetMorale.MoraleDamage -= Result;
		}
	}
}
