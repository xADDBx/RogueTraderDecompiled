using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem.Rules.Starships;

public class RuleStarshipPerformIncreaseMilitaryRating : RulebookTargetEvent<StarshipEntity>
{
	public int Result { get; private set; }

	public RuleStarshipPerformIncreaseMilitaryRating([NotNull] MechanicEntity initiator, [NotNull] StarshipEntity target, int value)
		: base(initiator, target)
	{
		Result = value;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		base.Target.Hull.CurrentMilitaryRating += Result;
	}
}
