using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISectorMapWarpTravelRepeatedEventHandler : ISubscriber
{
	void HandleStartJumpEvent();
}
