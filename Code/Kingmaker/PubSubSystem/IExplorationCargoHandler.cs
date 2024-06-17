using Kingmaker.Globalmap.Interaction;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IExplorationCargoHandler : ISubscriber
{
	void HandlePointOfInterestCargoInteraction(PointOfInterestCargo pointOfInterest);
}
