using System.Collections.Generic;
using System.Linq;
using Kingmaker.Localization;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using UnityEngine;

namespace Kingmaker.UI.Models.SettingsUI;

[CreateAssetMenu(menuName = "Blueprints/Settings UI/SettingsGroup")]
public class UISettingsGroup : ScriptableObject
{
	public LocalizedString Title;

	public UISettingsEntityBase.UISettingsPlatform SettingPlatform;

	public UISettingsEntityBase[] SettingsList;

	public bool IsVisible => IsVisibleOnPlatform(SettingPlatform);

	public List<UISettingsEntityBase> VisibleSettingsList => SettingsList.Where(IsSettingVisible).ToList();

	private bool IsSettingVisible(UISettingsEntityBase setting)
	{
		if (setting.IsTrinityModeOnly && !IsTrinityMode())
		{
			return false;
		}
		return IsVisibleOnPlatform(setting.SettingsPlatform);
	}

	private bool IsVisibleOnPlatform(UISettingsEntityBase.UISettingsPlatform platform)
	{
		if (platform == UISettingsEntityBase.UISettingsPlatform.Hide)
		{
			return false;
		}
		bool flag = false;
		if (platform != 0 && !(platform == UISettingsEntityBase.UISettingsPlatform.Console && flag) && (platform != UISettingsEntityBase.UISettingsPlatform.PC || flag) && (platform != UISettingsEntityBase.UISettingsPlatform.GamepadAndPC || !Game.Instance.IsControllerGamepad || flag))
		{
			if (platform == UISettingsEntityBase.UISettingsPlatform.PCMouseOnly && Game.Instance.IsControllerMouse)
			{
				return !flag;
			}
			return false;
		}
		return true;
	}

	private static bool IsTrinityMode()
	{
		return false;
	}
}
