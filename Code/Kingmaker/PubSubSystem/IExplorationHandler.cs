using Kingmaker.Globalmap.Interaction;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IExplorationHandler : ISubscriber
{
	void HandlePointOfInterestInteracted(BasePointOfInterest pointOfInterest);
}
