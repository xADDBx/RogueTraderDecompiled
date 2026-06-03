using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Settings;
using Kingmaker.Tutorial;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UISwitchJoyConAsMouse : IUISettingsSheet
{
	public UISettingsEntityBool JoyConActivate;

	[SerializeField]
	[ValidateNotNull]
	public BlueprintTutorial.Reference JoyConDeattachTutorial;

	public void LinkToSettings()
	{
		JoyConActivate.LinkSetting(SettingsRoot.Game.Switch.SwitchJoyConAsMouse);
		SettingsRoot.Game.Switch.JoyConDeattachTutorial = JoyConDeattachTutorial;
	}

	public void InitializeSettings()
	{
	}

	public void UpdateInteractable()
	{
		JoyConActivate.ModificationAllowedCheck = RootUIContext.CanChangeInput;
		JoyConActivate.ModificationAllowedReason = UIStrings.Instance.InteractableSettingsReasons.CannotSwitchJoyConInputBecause;
	}
}
