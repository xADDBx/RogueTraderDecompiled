using System.Collections.Generic;
using Code.Utility.ExtendedModInfo;

namespace Kingmaker.Modding;

public interface IModManagerBridge
{
	bool? DoorstopUsed { get; }

	void TryStart(string passedGameVersion);

	void TryStartUI();

	void OpenModInfoWindow(string modId);

	void CheckForUpdates();

	ExtendedModInfo GetModInfo(string modId);

	List<ExtendedModInfo> GetAllModsInfo();

	void EnableMod(string modId, bool state);
}
