using System;
using Kingmaker.Localization;
using Kingmaker.Settings;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIInteractableSettingsReasons
{
	public LocalizedString CannotChangeLanguageBecause;

	public LocalizedString CannotChangeOnlyOneSaveBecause;

	public LocalizedString CannotSwitchOnOnlyOneSave;

	public LocalizedString CannotChangeFrameRateLimitEnabledBecause;

	public LocalizedString CannotChangeFrameRateLimitBecause;

	public LocalizedString CannotChangeFsrSharpnessBecause;

	public LocalizedString CannotChangeAntialiasingQualityBecause;

	public string GetLabelByOrigin(SettingsNotInteractableReasonType reasonType)
	{
		return reasonType switch
		{
			SettingsNotInteractableReasonType.Language => CannotChangeLanguageBecause, 
			SettingsNotInteractableReasonType.OnlyOneSave => CannotChangeOnlyOneSaveBecause, 
			SettingsNotInteractableReasonType.OnlyOneSaveSwitchOn => CannotSwitchOnOnlyOneSave, 
			SettingsNotInteractableReasonType.FrameRateLimitEnabled => CannotChangeFrameRateLimitEnabledBecause, 
			SettingsNotInteractableReasonType.FrameRateLimit => CannotChangeFrameRateLimitBecause, 
			SettingsNotInteractableReasonType.FsrSharpness => CannotChangeFsrSharpnessBecause, 
			SettingsNotInteractableReasonType.AntialiasingQuality => CannotChangeAntialiasingQualityBecause, 
			_ => string.Empty, 
		};
	}
}
