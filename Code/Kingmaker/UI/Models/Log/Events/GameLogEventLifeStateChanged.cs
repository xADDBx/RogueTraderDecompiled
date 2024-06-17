using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventLifeStateChanged : GameLogEvent<GameLogEventLifeStateChanged>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber
	{
		public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
		{
			AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
			if (abstractUnitEntity != null)
			{
				AddEvent(new GameLogEventLifeStateChanged(abstractUnitEntity, prevLifeState));
			}
		}
	}

	public readonly AbstractUnitEntity Unit;

	public readonly UnitLifeState PrevLifeState;

	public GameLogEventLifeStateChanged(AbstractUnitEntity unit, UnitLifeState prevLifeState)
	{
		Unit = unit;
		PrevLifeState = prevLifeState;
	}
}
