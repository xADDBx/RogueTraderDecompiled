using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IStarSystemShipMovementHandler : ISubscriber
{
	void HandleStarSystemShipMovementStarted();

	void HandleStarSystemShipMovementEnded();
}
