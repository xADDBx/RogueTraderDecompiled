using Core.Cheats;
using Kingmaker.DLC;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;

namespace Kingmaker.Cheats;

internal static class DlcCheats
{
	[Cheat(Name = "refresh_all_dlc_statuses", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RefreshAllDLCStatuses()
	{
		StoreManager.RefreshAllDLCStatuses();
	}

	[Cheat(Name = "set_dlc_enabled", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetDlcEnabled(BlueprintDlc dlc)
	{
		StoreManager.UpdateDLCStatus(dlc);
		SetDlcStatus(dlc, isEnabled: true);
	}

	[Cheat(Name = "set_dlc_disabled", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetDlcDisabled(BlueprintDlc dlc)
	{
		StoreManager.UpdateDLCStatus(dlc);
		SetDlcStatus(dlc, isEnabled: false);
	}

	[Cheat(Name = "check_dlc_status", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static string CheckDlcStatus(BlueprintDlc dlc)
	{
		string dlcStatus = GetDlcStatus(dlc);
		PFLog.Default.Log(dlcStatus);
		return dlcStatus;
	}

	[Cheat(Name = "check_all_dlc_statuses", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheckAllDlcStatuses()
	{
		foreach (IBlueprintDlc dlc in Game.Instance.BlueprintRoot.DlcSettings.Dlcs)
		{
			CheckDlcStatus(dlc as BlueprintDlc);
		}
	}

	[Cheat(Name = "set_all_dlc_enabled", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetAllDlcEnabled()
	{
		foreach (IBlueprintDlc dlc in Game.Instance.BlueprintRoot.DlcSettings.Dlcs)
		{
			SetDlcEnabled(dlc as BlueprintDlc);
		}
	}

	[Cheat(Name = "set_all_dlc_disabled", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetAllDlcDisabled()
	{
		foreach (IBlueprintDlc dlc in Game.Instance.BlueprintRoot.DlcSettings.Dlcs)
		{
			SetDlcDisabled(dlc as BlueprintDlc);
		}
	}

	private static string GetDlcStatus(BlueprintDlc dlc)
	{
		IDLCStatus iDLCStatus = StoreManager.DLCCache.Get(dlc);
		if (iDLCStatus == null)
		{
			return $"Dlc {dlc} status is null";
		}
		return $"Dlc {dlc} status is Purchased={iDLCStatus.Purchased}, DownloadState={iDLCStatus.DownloadState}, IsMounted={iDLCStatus.IsMounted}, IsEnabled={dlc.IsEnabled}";
	}

	private static void SetDlcStatus(BlueprintDlc dlc, bool isEnabled)
	{
		if (dlc.IsEnabled == isEnabled)
		{
			PFLog.Default.Error($"Can not set {dlc} status cause it is already IsEnabled={isEnabled}");
			return;
		}
		if (isEnabled)
		{
			DlcStoreCheat.EnableDlc(dlc);
		}
		else
		{
			DlcStoreCheat.DisableDlc(dlc);
		}
		StoreManager.UpdateDLCStatus(dlc);
	}
}
