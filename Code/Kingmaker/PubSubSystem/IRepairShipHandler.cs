using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IRepairShipHandler : ISubscriber
{
	void HandleCantFullyRepairShip();
}
