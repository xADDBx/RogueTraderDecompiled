using System.Linq;
using Kingmaker.Designers;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventItemsCollection : GameLogEvent<GameLogEventItemsCollection>
{
	public enum EventType
	{
		Added,
		Removed
	}

	private class EventsHandler : GameLogController.GameEventsHandler, IItemsCollectionHandler, ISubscriber
	{
		public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
		{
			TryAddEvent(EventType.Added, collection, item, count);
		}

		public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
		{
			TryAddEvent(EventType.Removed, collection, item, count);
		}

		private void TryAddEvent(EventType type, ItemsCollection collection, ItemEntity item, int count)
		{
			if (collection == GameHelper.GetPlayerCharacter()?.Inventory.Collection && item.IsLootable && !(item?.Owner?.GetBodyOptional()?.AdditionalLimbs?.Contains(item.HoldingSlot)).GetValueOrDefault())
			{
				AddEvent(new GameLogEventItemsCollection(type, collection, item, count));
			}
		}
	}

	public readonly EventType Event;

	public readonly ItemsCollection Collection;

	public readonly ItemEntity Item;

	public readonly int Count;

	public GameLogEventItemsCollection(EventType @event, ItemsCollection collection, ItemEntity item, int count)
	{
		Event = @event;
		Collection = collection;
		Item = item;
		Count = count;
	}
}
