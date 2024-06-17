using Kingmaker.Globalmap.Interaction;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IExplorationExpeditionHandler : ISubscriber
{
	void HandlePointOfInterestExpeditionInteraction(PointOfInterestExpedition pointOfInterest);
}
