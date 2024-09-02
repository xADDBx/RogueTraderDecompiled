using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IPlayerOpenItemDescriptionHandler : ISubscriber<IItemEntity>, ISubscriber
{
	void HandlePlayerOpenItemDescription();
}
