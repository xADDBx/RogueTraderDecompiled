using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IStarShipLandingHandler : ISubscriber
{
	void HandleStarShipLanded(StarSystemObjectView sso);
}
