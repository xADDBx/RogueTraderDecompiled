using System;
using System.Collections.Generic;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet.Controls;

[Serializable]
public class UIKeybindDialogSettings : IUISettingsSheet
{
	public List<UISettingsEntityKeyBinding> DialogChoices;

	public UISettingsEntityKeyBinding NextOrEnd;

	public void LinkToSettings()
	{
		for (int i = 0; i < DialogChoices.Count; i++)
		{
			DialogChoices[i].LinkSetting(SettingsRoot.Controls.Keybindings.Dialog.DialogChoices[i]);
		}
		NextOrEnd.LinkSetting(SettingsRoot.Controls.Keybindings.Dialog.NextOrEnd);
	}

	public void InitializeSettings()
	{
	}
}
