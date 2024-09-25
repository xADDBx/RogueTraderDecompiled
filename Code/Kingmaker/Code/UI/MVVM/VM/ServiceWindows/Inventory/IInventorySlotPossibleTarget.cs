using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;

public interface IInventorySlotPossibleTarget : ISubscriber<ItemEntity>, ISubscriber
{
	void HandleHighlightStart(ItemSlot item);

	void HandleHighlightStop();
}
