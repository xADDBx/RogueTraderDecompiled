using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface ISplitItemHandler : ISubscriber
{
	void HandleSplitItem();

	void HandleAfterSplitItem(ItemEntity item);

	void HandleBeforeSplitItem(ItemEntity item, ItemsCollection from, ItemsCollection to);
}
