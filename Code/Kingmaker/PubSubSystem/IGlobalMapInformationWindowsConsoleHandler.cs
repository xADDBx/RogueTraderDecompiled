using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGlobalMapInformationWindowsConsoleHandler : ISubscriber
{
	void HandleShowSystemInformationWindowConsole(SectorMapObjectEntity sectorMapObjectEntity);

	void HandleHideSystemInformationWindowConsole();

	void HandleShowAllSystemsInformationWindowConsole(SectorMapObjectEntity sectorMapObjectEntity);

	void HandleHideAllSystemsInformationWindowConsole();

	void HandleChangeInformationWindowsConsole();

	void HandleChangeCurrentSystemInfoConsole(SectorMapObjectEntity sectorMapObjectEntity);
}
