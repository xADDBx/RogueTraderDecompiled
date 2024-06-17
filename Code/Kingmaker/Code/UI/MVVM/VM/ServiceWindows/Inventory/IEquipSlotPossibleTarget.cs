using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;

public interface IEquipSlotPossibleTarget : ISubscriber<ItemEntity>, ISubscriber
{
	void HandleHighlightStart(ItemEntity item);

	void HandleHighlightStop();
}
