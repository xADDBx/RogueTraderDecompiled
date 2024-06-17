using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UIGameMainSettings : IUISettingsSheet
{
	public UISettingsEntityDropdownLocale Localization;

	public UISettingsEntityBool AutofillActionbarSlots;

	public UISettingsEntityBool LootInCombat;

	public UISettingsEntityBool SendGameStatistic;

	public UISettingsEntityBool SendSaves;

	public UISettingsEntityBool UseHotAreas;

	public UISettingsEntityBool BloodOnCharacters;

	public UISettingsEntityBool DismemberCharacters;

	public UISettingsEntityBool AcceleratedMove;

	public void LinkToSettings()
	{
		Localization.LinkSetting(SettingsRoot.Game.Main.Localization);
		AutofillActionbarSlots.LinkSetting(SettingsRoot.Game.Main.AutofillActionbarSlots);
		LootInCombat.LinkSetting(SettingsRoot.Game.Main.LootInCombat);
		SendGameStatistic.LinkSetting(SettingsRoot.Game.Main.SendGameStatistic);
		SendSaves.LinkSetting(SettingsRoot.Game.Main.SendSaves);
		UseHotAreas.LinkSetting(SettingsRoot.Game.Main.UseHotAreas);
		BloodOnCharacters.LinkSetting(SettingsRoot.Game.Main.BloodOnCharacters);
		DismemberCharacters.LinkSetting(SettingsRoot.Game.Main.DismemberCharacters);
		AcceleratedMove.LinkSetting(SettingsRoot.Game.Main.AcceleratedMove);
	}

	public void InitializeSettings()
	{
	}

	public void UpdateInteractable()
	{
		Localization.ModificationAllowedCheck = () => RootUIContext.CanChangeLanguage();
		Localization.ModificationAllowedReason = UIStrings.Instance.InteractableSettingsReasons.GetLabelByOrigin(SettingsNotInteractableReasonType.Language);
	}
}
