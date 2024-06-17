using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IScanStarSystemObjectHandler : ISubscriber<StarSystemObjectEntity>, ISubscriber
{
	void HandleStartScanningStarSystemObject();

	void HandleScanStarSystemObject();
}
