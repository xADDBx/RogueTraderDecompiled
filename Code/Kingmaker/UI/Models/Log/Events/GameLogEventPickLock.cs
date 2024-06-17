using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventPickLock : GameLogEvent<GameLogEventPickLock>
{
	public enum ResultType
	{
		Success,
		Fail,
		CriticalFail
	}

	private class EventsHandler : GameLogController.GameEventsHandler, IPickLockHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
	{
		public void HandlePickLockSuccess(MapObjectView mapObjectView)
		{
			AddEvent(EventInvokerExtensions.BaseUnitEntity, mapObjectView, ResultType.Success);
		}

		public void HandlePickLockFail(MapObjectView mapObjectView, bool critical)
		{
			AddEvent(EventInvokerExtensions.BaseUnitEntity, mapObjectView, (!critical) ? ResultType.Fail : ResultType.CriticalFail);
		}

		private void AddEvent(BaseUnitEntity unit, MapObjectView mapObject, ResultType result)
		{
			AddEvent(new GameLogEventPickLock(unit, mapObject, result));
		}
	}

	public readonly BaseUnitEntity Actor;

	public readonly MapObjectView MapObject;

	public readonly ResultType Result;

	private GameLogEventPickLock(BaseUnitEntity actor, MapObjectView mapObject, ResultType result)
	{
		Actor = actor;
		MapObject = mapObject;
		Result = result;
	}

	protected override bool TrySwallowEventInternal(GameLogEvent @event)
	{
		return base.TrySwallowEventInternal(@event);
	}
}
