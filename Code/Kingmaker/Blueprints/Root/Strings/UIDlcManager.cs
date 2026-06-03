using System;
using Kingmaker.DLC;
using Kingmaker.Localization;
using Kingmaker.Stores;
using Kingmaker.Utility;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIDlcManager
{
	public LocalizedString DlcManagerLabel;

	public LocalizedString ModsLabel;

	public LocalizedString AvailableForPurchase;

	public LocalizedString Purchase;

	public LocalizedString Purchased;

	public LocalizedString ComingSoon;

	public LocalizedString DlcDownloading;

	public LocalizedString DlcBoughtAndNotInstalled;

	public LocalizedString Install;

	public LocalizedString YouDontHaveThisDlc;

	public LocalizedString YouDontHaveThisStory;

	public LocalizedString StoryCompanyIs;

	public LocalizedString StoryDlc;

	public LocalizedString AdditionalContentDlc;

	public LocalizedString CosmeticDlc;

	public LocalizedString PromotionalDlc;

	public LocalizedString DlcStatus;

	public LocalizedString DlcSwitchOnOffHint;

	public LocalizedString CannotChangeModSwitchState;

	public LocalizedString RestartChangeModConfirmation;

	public LocalizedString RestartGame;

	public LocalizedString NeedToUpdateThisMod;

	public LocalizedString ModChangedNeedToReloadGame;

	public LocalizedString InstalledMods;

	public LocalizedString YouDontHaveAnyMods;

	public LocalizedString DiscoverMoreMods;

	public LocalizedString NexusMods;

	public LocalizedString SteamWorkshop;

	public LocalizedString ResetAllModsToPreviousState;

	public LocalizedString ModSettings;

	public LocalizedString NeedRestartAfterPurchase;

	public LocalizedString NeedWaitAllDlcsDownload;

	public LocalizedString DeleteDlc;

	public LocalizedString AreYouSureDeleteDlc;

	public LocalizedString PlayVideo;

	public LocalizedString PauseVideo;

	public LocalizedString StopVideo;

	public LocalizedString CannotChangeDlcSwitchState;

	public LocalizedString CannotChangeDlcSwitchStateRightNowBecauseSaveNotAllowed;

	public LocalizedString YouSwitchDlcOnAndCantDoItBack;

	public LocalizedString ResetAllDlcsToPreviousState;

	public LocalizedString InstalledDlcs;

	public LocalizedString YouDontHaveAnyInstalledDlcs;

	public LocalizedString ThisSettingWillAffectCurrentSave;

	public LocalizedString NewDlcAfterLoadingMessageBoxHint;

	public LocalizedString SwitchAvailableForPurchase;

	public LocalizedString SwitchPurchased;

	public LocalizedString SwitchPurchase;

	public LocalizedString SwitchCosmeticDlc;

	public string GetPurchaseLabel()
	{
		return ApplicationHelper.IsRunningOnAnySwitch ? SwitchPurchase : Purchase;
	}

	public string GetPurchasedLabel()
	{
		return ApplicationHelper.IsRunningOnAnySwitch ? SwitchPurchased : Purchased;
	}

	public string GetAvailableForPurchaseLabel()
	{
		return ApplicationHelper.IsRunningOnAnySwitch ? SwitchAvailableForPurchase : AvailableForPurchase;
	}

	public string GetDlcTypeLabel(DlcTypeEnum type)
	{
		return type switch
		{
			DlcTypeEnum.StoryDlc => StoryDlc, 
			DlcTypeEnum.AdditionalContentDlc => AdditionalContentDlc, 
			DlcTypeEnum.CosmeticDlc => ApplicationHelper.IsRunningOnAnySwitch ? SwitchCosmeticDlc : CosmeticDlc, 
			DlcTypeEnum.PromotionalDlc => PromotionalDlc, 
			_ => string.Empty, 
		};
	}

	public string GetDlcPurchaseStateLabel(BlueprintDlc.DlcPurchaseState state)
	{
		return state switch
		{
			BlueprintDlc.DlcPurchaseState.Purchased => GetPurchasedLabel(), 
			BlueprintDlc.DlcPurchaseState.AvailableToPurchase => GetAvailableForPurchaseLabel(), 
			BlueprintDlc.DlcPurchaseState.ComingSoon => ComingSoon, 
			_ => string.Empty, 
		};
	}
}
