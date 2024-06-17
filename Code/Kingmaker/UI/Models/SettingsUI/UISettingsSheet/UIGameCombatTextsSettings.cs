using System;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UIGameCombatTextsSettings : IUISettingsSheet
{
	public UISettingsEntityDropdownEntitiesType ShowSpellName;

	public UISettingsEntityDropdownEntitiesType ShowAvoid;

	public UISettingsEntityDropdownEntitiesType ShowMiss;

	public UISettingsEntityDropdownEntitiesType ShowAttackOfOpportunity;

	public UISettingsEntityDropdownEntitiesType ShowCriticalHit;

	public UISettingsEntityDropdownEntitiesType ShowSneakAttack;

	public UISettingsEntityDropdownEntitiesType ShowDamage;

	public UISettingsEntityDropdownEntitiesType ShowSaves;

	public void LinkToSettings()
	{
		ShowSpellName.LinkSetting(SettingsRoot.Game.CombatTexts.ShowSpellName);
		ShowAvoid.LinkSetting(SettingsRoot.Game.CombatTexts.ShowAvoid);
		ShowMiss.LinkSetting(SettingsRoot.Game.CombatTexts.ShowMiss);
		ShowAttackOfOpportunity.LinkSetting(SettingsRoot.Game.CombatTexts.ShowAttackOfOpportunity);
		ShowCriticalHit.LinkSetting(SettingsRoot.Game.CombatTexts.ShowCriticalHit);
		ShowSneakAttack.LinkSetting(SettingsRoot.Game.CombatTexts.ShowSneakAttack);
		ShowDamage.LinkSetting(SettingsRoot.Game.CombatTexts.ShowDamage);
		ShowSaves.LinkSetting(SettingsRoot.Game.CombatTexts.ShowSaves);
	}

	public void InitializeSettings()
	{
	}
}
