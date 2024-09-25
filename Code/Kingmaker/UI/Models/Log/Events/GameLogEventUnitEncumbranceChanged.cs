using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventUnitEncumbranceChanged : GameLogEvent<GameLogEventUnitEncumbranceChanged>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IUnitEncumbranceHandler, ISubscriber
	{
		public void ChangeUnitEncumbrance(Encumbrance prevEncumbrance)
		{
			BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
			AddEvent(new GameLogEventUnitEncumbranceChanged(baseUnitEntity, baseUnitEntity.EncumbranceData?.Value ?? Encumbrance.Light, prevEncumbrance));
		}
	}

	public readonly BaseUnitEntity Unit;

	public readonly Encumbrance CurrentEncumbrance;

	public readonly Encumbrance PreviousEncumbrance;

	public GameLogEventUnitEncumbranceChanged(BaseUnitEntity unit, Encumbrance currentEncumbrance, Encumbrance previousEncumbrance)
	{
		Unit = unit;
		CurrentEncumbrance = currentEncumbrance;
		PreviousEncumbrance = previousEncumbrance;
	}
}
