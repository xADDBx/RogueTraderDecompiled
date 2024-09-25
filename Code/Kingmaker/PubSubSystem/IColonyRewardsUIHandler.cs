using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IColonyRewardsUIHandler : ISubscriber
{
	void HandleColonyRewardsShow(Colony colony);
}
