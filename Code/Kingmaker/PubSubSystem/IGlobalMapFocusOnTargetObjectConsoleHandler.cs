using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGlobalMapFocusOnTargetObjectConsoleHandler : ISubscriber
{
	void HandleChangeFocusOnTargetObjectConsole(SectorMapObjectEntity sectorMapObjectEntity);
}
