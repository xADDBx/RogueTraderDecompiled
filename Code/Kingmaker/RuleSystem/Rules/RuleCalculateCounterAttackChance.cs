using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateCounterAttackChance : RulebookEvent
{
	[CanBeNull]
	public readonly MechanicEntity MaybeTarget;

	public int Result { get; private set; }

	public bool SuppressedByShield { get; set; }

	public RuleCalculateCounterAttackChance([NotNull] MechanicEntity initiator, MechanicEntity target)
		: base(initiator)
	{
		MaybeTarget = target;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Result = ((!SuppressedByShield) ? 1 : 0);
	}
}
