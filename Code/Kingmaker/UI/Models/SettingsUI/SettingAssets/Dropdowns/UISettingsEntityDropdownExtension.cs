namespace Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

public static class UISettingsEntityDropdownExtension
{
	public static int ValuesCount(this IUISettingsEntityDropdown settingsEntityDropdown)
	{
		return settingsEntityDropdown.LocalizedValues.Count;
	}
}
