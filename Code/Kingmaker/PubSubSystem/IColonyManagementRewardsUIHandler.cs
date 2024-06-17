using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IColonyManagementRewardsUIHandler : ISubscriber
{
	void HandleColonyRewardsShow();
}
