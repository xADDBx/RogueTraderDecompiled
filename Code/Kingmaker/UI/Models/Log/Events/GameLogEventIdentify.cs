using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventIdentify : GameLogEvent<GameLogEventIdentify>
{
	public enum ResultType
	{
		Success,
		Fail
	}

	private class EventsHandler : GameLogController.GameEventsHandler, IIdentifyHandler, ISubscriber<IItemEntity>, ISubscriber
	{
		public void OnItemIdentified(BaseUnitEntity character)
		{
			AddEvent(new GameLogEventIdentify(ResultType.Success, EventInvokerExtensions.GetEntity<ItemEntity>(), character));
		}

		public void OnFailedToIdentify()
		{
			AddEvent(new GameLogEventIdentify(ResultType.Fail, EventInvokerExtensions.GetEntity<ItemEntity>(), null));
		}
	}

	public readonly ResultType Result;

	public readonly ItemEntity TargetItem;

	public readonly BaseUnitEntity Actor;

	public GameLogEventIdentify(ResultType result, ItemEntity targetItem, BaseUnitEntity actor)
	{
		Result = result;
		TargetItem = targetItem;
		Actor = actor;
	}
}
