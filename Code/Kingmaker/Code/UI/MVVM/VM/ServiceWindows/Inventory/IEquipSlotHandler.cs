using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;

public interface IEquipSlotHandler : ISubscriber
{
	void ChooseSlotToItem(InventorySlotConsoleView item);
}
