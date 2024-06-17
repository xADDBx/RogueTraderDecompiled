using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface ICollectLootHandler : ISubscriber
{
	void HandleCollectAll(ItemsCollection from, ItemsCollection to);
}
