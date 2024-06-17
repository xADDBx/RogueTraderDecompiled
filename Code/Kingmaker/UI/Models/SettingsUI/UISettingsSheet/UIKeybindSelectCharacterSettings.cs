using System;
using System.Collections.Generic;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UIKeybindSelectCharacterSettings : IUISettingsSheet
{
	public List<UISettingsEntityKeyBinding> SelectCharacter;

	public UISettingsEntityKeyBinding SelectAll;

	public void LinkToSettings()
	{
		for (int i = 0; i < SelectCharacter.Count; i++)
		{
			SelectCharacter[i].LinkSetting(SettingsRoot.Controls.Keybindings.SelectCharacter.SelectCharacter[i]);
		}
		SelectAll.LinkSetting(SettingsRoot.Controls.Keybindings.SelectCharacter.SelectAll);
	}

	public void InitializeSettings()
	{
	}
}
