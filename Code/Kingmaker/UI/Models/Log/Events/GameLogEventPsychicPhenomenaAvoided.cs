using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventPsychicPhenomenaAvoided : GameLogEvent<GameLogEventPsychicPhenomenaAvoided>
{
	[UsedImplicitly]
	private class EventsHandler : GameLogController.GameEventsHandler, IPsychicPhenomenaHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
	{
		public void HandlePsychicPhenomena(RuleCalculatePsychicPhenomenaEffect rule)
		{
			if (rule.PerilsOfTheWarpAvoid.IsAvoided || rule.PsychicPhenomenaAvoid.IsAvoided)
			{
				AddEvent(new GameLogEventPsychicPhenomenaAvoided(rule));
			}
		}
	}

	public readonly RuleCalculatePsychicPhenomenaEffect Rule;

	public GameLogEventPsychicPhenomenaAvoided(RuleCalculatePsychicPhenomenaEffect rule)
	{
		Rule = rule;
	}
}
