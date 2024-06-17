using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IPlayerOpenItemDescriptionFirstTimeHandler : ISubscriber<IItemEntity>, ISubscriber
{
	void HandlePlayerOpenItemDescriptionFirstTime();
}
