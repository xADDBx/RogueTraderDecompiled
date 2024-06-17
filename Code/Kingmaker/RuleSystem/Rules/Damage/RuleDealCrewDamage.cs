using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class RuleDealCrewDamage : RulebookTargetEvent<StarshipEntity>
{
	public int Result { get; private set; }

	public RuleDealCrewDamage([NotNull] MechanicEntity initiator, [NotNull] StarshipEntity target, int damage)
		: base(initiator, target)
	{
		Result = damage;
	}

	public RuleDealCrewDamage([NotNull] IMechanicEntity initiator, [NotNull] StarshipEntity target, int damage)
		: this((MechanicEntity)initiator, target, damage)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		base.Target.Crew.Damage(Result);
	}
}
