using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICargoRewardsUIHandler : ISubscriber
{
	void HandleCargoRewardsShow();

	void HandleCargoRewardsHide();
}
