using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface IInventoryHandler : ISubscriber
{
	void Refresh();

	void TryEquip(ItemSlotVM slot);

	void TryDrop(ItemSlotVM slot);

	void TryMoveToCargo(ItemSlotVM slot, bool immediately = false);

	void TryMoveToInventory(ItemSlotVM slot, bool immediately = false);
}
