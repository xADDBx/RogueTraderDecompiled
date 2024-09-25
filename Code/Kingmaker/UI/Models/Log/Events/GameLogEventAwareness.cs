using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventAwareness : GameLogEvent<GameLogEventAwareness>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IAwarenessHandler, ISubscriber<IMapObjectEntity>, ISubscriber
	{
		public void OnEntityNoticed(BaseUnitEntity character)
		{
			MapObjectEntity entity = EventInvokerExtensions.GetEntity<MapObjectEntity>();
			AddEvent(new GameLogEventAwareness(character, entity));
		}
	}

	public readonly BaseUnitEntity Actor;

	public readonly MapObjectEntity TargetObject;

	public GameLogEventAwareness(BaseUnitEntity actor, MapObjectEntity targetObject)
	{
		Actor = actor;
		TargetObject = targetObject;
	}
}
