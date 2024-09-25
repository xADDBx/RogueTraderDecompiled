using System;
using Kingmaker.Localization;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIControllerModeTexts
{
	public LocalizedString GamepadConnectedHeaderText;

	[FormerlySerializedAs("GamepadDisconnectedText")]
	public LocalizedString GamepadDisconnectedHeaderText;

	public LocalizedString GamepadConnectedText;

	public LocalizedString GamepadDisconnectedText;

	[FormerlySerializedAs("GamepadConnectedGuideText")]
	public LocalizedString ConfirmSwitchText;

	public LocalizedString CantChangeInput;

	public LocalizedString ChangeInputProcess;

	public LocalizedString PressAnyKeyText;
}
