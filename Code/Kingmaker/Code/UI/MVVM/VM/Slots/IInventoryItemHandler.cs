using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface IInventoryItemHandler : ISubscriber
{
	void HandleChangeItem(EquipSlotVM slot);
}
