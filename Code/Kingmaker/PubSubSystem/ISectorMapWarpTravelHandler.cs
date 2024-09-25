using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISectorMapWarpTravelHandler : ISubscriber<ISectorMapObjectEntity>, ISubscriber
{
	void HandleWarpTravelBeforeStart();

	void HandleWarpTravelStarted(SectorMapPassageEntity passage);

	void HandleWarpTravelStopped();

	void HandleWarpTravelPaused();

	void HandleWarpTravelResumed();
}
