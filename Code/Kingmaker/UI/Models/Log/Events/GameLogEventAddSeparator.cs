using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.UI.Models.Log.Events;

public sealed class GameLogEventAddSeparator : GameLogEvent<GameLogEventAddSeparator>
{
	public enum States
	{
		Start,
		Break,
		Finish
	}

	private class EventsHandler : GameLogController.GameEventsHandler, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber
	{
		public void HandleExecutionProcessStart(AbilityExecutionContext context)
		{
			if (!context.DisableLog)
			{
				AddEvent(new GameLogEventAddSeparator(States.Start));
			}
		}

		public void HandleExecutionProcessEnd(AbilityExecutionContext context)
		{
			if (!context.DisableLog)
			{
				AddEvent(new GameLogEventAddSeparator(States.Finish));
			}
		}
	}

	public States State { get; private set; }

	private GameLogEventAddSeparator()
	{
	}

	private GameLogEventAddSeparator(States state)
	{
		State = state;
	}
}
