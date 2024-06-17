using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IFactCollectionUpdatedHandler : ISubscriber
{
	void HandleFactCollectionUpdated(EntityFactsProcessor collection);
}
