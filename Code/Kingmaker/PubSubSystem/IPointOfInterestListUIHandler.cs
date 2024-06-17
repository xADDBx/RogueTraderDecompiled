using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IPointOfInterestListUIHandler : ISubscriber
{
	void HandlePointOfInterestUpdated();
}
