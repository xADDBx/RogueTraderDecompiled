using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IItemsCollectionHandler : ISubscriber
{
	void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count);

	void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count);
}
