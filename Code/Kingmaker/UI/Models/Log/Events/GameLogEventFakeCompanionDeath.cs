using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventFakeCompanionDeath : GameLogEvent<GameLogEventFakeCompanionDeath>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IUnitFakeDeathMessageHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
	{
		public void HandleUnitFakeDeathMessage(LocalizedString fakeDeathMessage)
		{
			AddEvent(new GameLogEventFakeCompanionDeath(EventInvokerExtensions.BaseUnitEntity, fakeDeathMessage));
		}
	}

	public readonly BaseUnitEntity Unit;

	public readonly LocalizedString Message;

	public GameLogEventFakeCompanionDeath(BaseUnitEntity unit, LocalizedString message)
	{
		Unit = unit;
		Message = message;
	}
}
