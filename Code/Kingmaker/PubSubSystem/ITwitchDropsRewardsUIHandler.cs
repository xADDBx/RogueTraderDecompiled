using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ITwitchDropsRewardsUIHandler : ISubscriber
{
	void HandleItemRewardsShow();
}
