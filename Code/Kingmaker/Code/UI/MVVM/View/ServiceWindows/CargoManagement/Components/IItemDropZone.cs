using Kingmaker.Code.UI.MVVM.VM.Slots;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public interface IItemDropZone
{
	bool Interactable { get; }

	void TryDropItem(ItemSlotVM itemVM);
}
