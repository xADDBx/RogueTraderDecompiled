using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICanAccessStarshipInventoryHandler : ISubscriber
{
	void HandleCanAccessStarshipInventory();
}
