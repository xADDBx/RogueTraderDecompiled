using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISectorMapScanHandler : ISubscriber<ISectorMapObjectEntity>, ISubscriber
{
	void HandleScanStarted(float range, float duration);

	void HandleSectorMapObjectScanned([CanBeNull] SectorMapPassageView passageToStarSystem);

	void HandleScanStopped();
}
