using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.RuleSystem.Rules.Starships;

public class RuleStarshipPerformDecreaseMilitaryRating : RulebookTargetEvent<StarshipEntity>
{
	public int Result { get; private set; }

	public RuleStarshipPerformDecreaseMilitaryRating([NotNull] MechanicEntity initiator, [NotNull] StarshipEntity target, int value)
		: base(initiator, target)
	{
		Result = value;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		PartStarshipHull hull = base.Target.GetHull();
		Result = Math.Min(hull.CurrentMilitaryRating, Result);
		hull.CurrentMilitaryRating -= Result;
	}
}
