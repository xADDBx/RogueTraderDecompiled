using System.Collections.Generic;
using System.Linq;
using Kingmaker.Localization;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.Utility.BuildModeUtils;
using UnityEngine;

namespace Kingmaker.UI.Models.SettingsUI;

[CreateAssetMenu(menuName = "Blueprints/Settings UI/SettingsGroup")]
public class UISettingsGroup : ScriptableObject
{
	public LocalizedString Title;

	public UISettingsEntityBase.UISettingsPlatform SettingPlatform;

	public UISettingsEntityBase[] SettingsList;

	public bool IsVisible => SettingVisible(SettingPlatform);

	public List<UISettingsEntityBase> VisibleSettingsList => SettingsList.Where((UISettingsEntityBase s) => SettingVisible(s.SettingsPlatform)).ToList();

	private bool SettingVisible(UISettingsEntityBase.UISettingsPlatform platform)
	{
		if (platform == UISettingsEntityBase.UISettingsPlatform.Hide)
		{
			return false;
		}
		if (platform != (UISettingsEntityBase.UISettingsPlatform)((!BuildModeUtility.Data.CloudSwitchSettings) ? 1 : 2) && (platform != UISettingsEntityBase.UISettingsPlatform.PCMouseOnly || !Game.Instance.IsControllerMouse))
		{
			if (platform != 0)
			{
				return platform == UISettingsEntityBase.UISettingsPlatform.GamepadAndPC;
			}
			return true;
		}
		return true;
	}
}
