using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventUnitEquipment : GameLogEvent<GameLogEventPartyGainExperience>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber
	{
		public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
		{
			AddEvent(new GameLogEventUnitEquipment(slot, previousItem));
		}
	}

	public readonly ItemSlot Slot;

	public readonly ItemEntity PreviousItem;

	public GameLogEventUnitEquipment(ItemSlot slot, ItemEntity previousItem)
	{
		Slot = slot;
		PreviousItem = previousItem;
	}
}
