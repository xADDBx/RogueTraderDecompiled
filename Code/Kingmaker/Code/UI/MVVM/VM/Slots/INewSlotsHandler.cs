using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface INewSlotsHandler : ISubscriber
{
	void HandleTryInsertSlot(InsertableLootSlotVM slot);

	void HandleTryMoveSlot(ItemSlotVM from, ItemSlotVM to);

	void HandleTrySplitSlot(ItemSlotVM slot);
}
