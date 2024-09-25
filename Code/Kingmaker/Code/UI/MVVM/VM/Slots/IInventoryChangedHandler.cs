using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface IInventoryChangedHandler : ISubscriber
{
	void HandleSetItem(ItemEntity item, int oldIndex);

	void HandleUpdateItem(ItemEntity item, ItemsCollection collection, int index);

	void HandleRemoveItem(ItemEntity item, ItemsCollection from, int oldIndex);
}
