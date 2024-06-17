using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class RuleHealCrewDamage : RulebookTargetEvent<StarshipEntity>
{
	private int Amount;

	private bool HealAll;

	public int Result { get; private set; }

	public RuleHealCrewDamage([NotNull] MechanicEntity initiator, [NotNull] StarshipEntity target, int amount, bool healAll)
		: base(initiator, target)
	{
		Amount = amount;
		HealAll = healAll;
	}

	public RuleHealCrewDamage([NotNull] IMechanicEntity initiator, [NotNull] StarshipEntity target, int amount, bool healAll)
		: this((MechanicEntity)initiator, target, amount, healAll)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		base.Target.Crew.Heal(HealAll, Amount);
	}
}
