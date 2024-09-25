using Kingmaker.Cargo;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventCargoCollection : GameLogEvent<GameLogEventCargoCollection>
{
	public enum EventType
	{
		CargoCreated,
		CargoReplenished,
		CargoFormed,
		CargoRemoved
	}

	private class EventsHandler : GameLogController.GameEventsHandler, ICargoStateChangedHandler, ISubscriber
	{
		private void TryAddEvent(EventType type, CargoEntity cargo, ItemEntity item = null)
		{
			AddEvent(new GameLogEventCargoCollection(type, cargo, item));
		}

		public void HandleCreateNewCargo(CargoEntity entity)
		{
			TryAddEvent(EventType.CargoCreated, entity);
		}

		public void HandleAddItemToCargo(ItemEntity item, ItemsCollection from, CargoEntity to, int oldIndex)
		{
			if (from != Game.Instance.Player.Inventory)
			{
				TryAddEvent(EventType.CargoReplenished, to, item);
			}
			if (to.IsFull)
			{
				TryAddEvent(EventType.CargoFormed, to, item);
			}
		}

		public void HandleRemoveCargo(CargoEntity entity, bool fromMassSell)
		{
			TryAddEvent(EventType.CargoRemoved, entity);
		}

		public void HandleRemoveItemFromCargo(ItemEntity item, CargoEntity from)
		{
		}
	}

	public readonly CargoEntity Cargo;

	public readonly ItemEntity Item;

	public EventType Event { get; private set; }

	public int CountPercent { get; private set; }

	public GameLogEventCargoCollection(EventType @event, CargoEntity cargo, ItemEntity item = null)
	{
		Event = @event;
		Cargo = cargo;
		CountPercent = cargo.FilledVolumePercent;
		Item = item;
	}

	protected override bool TrySwallowEventInternal(GameLogEvent @event)
	{
		if (@event is GameLogEventCargoCollection gameLogEventCargoCollection && Cargo == gameLogEventCargoCollection.Cargo)
		{
			if (Event == EventType.CargoReplenished && Event == gameLogEventCargoCollection.Event)
			{
				CountPercent = gameLogEventCargoCollection.Cargo.FilledVolumePercent;
				if (Cargo.IsFull)
				{
					Event = EventType.CargoFormed;
				}
				return true;
			}
			if (Event == EventType.CargoFormed && Event == gameLogEventCargoCollection.Event)
			{
				return true;
			}
		}
		return base.TrySwallowEventInternal(@event);
	}
}
