using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface ITransferItemHandler : ISubscriber
{
	void HandleTransferItem(ItemsCollection from, ItemsCollection to);
}
