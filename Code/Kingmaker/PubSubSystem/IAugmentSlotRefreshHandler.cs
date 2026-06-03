using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAugmentSlotRefreshHandler : ISubscriber
{
	void HandleAugmentSlotRefresh();
}
