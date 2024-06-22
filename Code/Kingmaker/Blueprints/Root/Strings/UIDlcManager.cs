using System;
using Kingmaker.DLC;
using Kingmaker.Localization;
using Kingmaker.Stores;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIDlcManager
{
	public LocalizedString DlcManagerLabel;

	public LocalizedString ModsLabel;

	public LocalizedString Purchase;

	public LocalizedString Purchased;

	public LocalizedString ComingSoon;

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

	public string GetDlcTypeLabel(DlcTypeEnum type)
	{
		return type switch
		{
			DlcTypeEnum.StoryDlc => StoryDlc, 
			DlcTypeEnum.AdditionalContentDlc => AdditionalContentDlc, 
			DlcTypeEnum.CosmeticDlc => CosmeticDlc, 
			DlcTypeEnum.PromotionalDlc => PromotionalDlc, 
			_ => string.Empty, 
		};
	}

	public string GetDlcPurchaseStateLabel(BlueprintDlc.DlcPurchaseState state)
	{
		return state switch
		{
			BlueprintDlc.DlcPurchaseState.Purchased => Purchased, 
			BlueprintDlc.DlcPurchaseState.AvailableToPurchase => UIStrings.Instance.ProfitFactorTexts.AvailableToUseValue, 
			BlueprintDlc.DlcPurchaseState.ComingSoon => ComingSoon, 
			_ => string.Empty, 
		};
	}
}
