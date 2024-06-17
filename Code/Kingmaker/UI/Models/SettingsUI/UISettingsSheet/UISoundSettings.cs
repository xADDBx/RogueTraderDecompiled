using System;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UISoundSettings : IUISettingsSheet
{
	public UISettingsEntitySliderFloat VolumeMaster;

	public UISettingsEntitySliderFloat VolumeVoices;

	public UISettingsEntitySliderFloat VolumeVoicesCharacterInGame;

	public UISettingsEntitySliderFloat VolumeVoicesNpcInGame;

	public UISettingsEntitySliderFloat VolumeVoicesDialogues;

	public UISettingsEntitySliderFloat VolumeMusic;

	public UISettingsEntitySliderFloat VolumeSfx;

	public UISettingsEntitySliderFloat VolumeAmbience;

	public UISettingsEntitySliderFloat VolumeAbilities;

	public UISettingsEntitySliderFloat VolumeRangedWeapons;

	public UISettingsEntitySliderFloat VolumeMeleeWeapons;

	public UISettingsEntitySliderFloat VolumeHitsLevel;

	public UISettingsEntitySliderFloat VolumeUI;

	public UISettingsEntityDropdownVoiceAskFrequency VoicedAskFrequency;

	public UISettingsEntityBool MuteAudioWhileTheGameIsOutFocus;

	public void LinkToSettings()
	{
		VolumeMaster.LinkSetting(SettingsRoot.Sound.VolumeMaster);
		VolumeVoices.LinkSetting(SettingsRoot.Sound.VolumeVoices);
		VolumeVoicesCharacterInGame.LinkSetting(SettingsRoot.Sound.VolumeVoicesCharacterInGame);
		VolumeVoicesNpcInGame.LinkSetting(SettingsRoot.Sound.VolumeVoicesNpcInGame);
		VolumeVoicesDialogues.LinkSetting(SettingsRoot.Sound.VolumeVoicesDialogues);
		VolumeMusic.LinkSetting(SettingsRoot.Sound.VolumeMusic);
		VolumeSfx.LinkSetting(SettingsRoot.Sound.VolumeSfx);
		VolumeAmbience.LinkSetting(SettingsRoot.Sound.VolumeAmbience);
		VolumeAbilities.LinkSetting(SettingsRoot.Sound.VolumeAbilities);
		VolumeRangedWeapons.LinkSetting(SettingsRoot.Sound.VolumeRangedWeapons);
		VolumeMeleeWeapons.LinkSetting(SettingsRoot.Sound.VolumeMeleeWeapons);
		VolumeHitsLevel.LinkSetting(SettingsRoot.Sound.VolumeHitsLevel);
		VolumeUI.LinkSetting(SettingsRoot.Sound.VolumeUI);
		VoicedAskFrequency.LinkSetting(SettingsRoot.Sound.VoicedAskFrequency);
		MuteAudioWhileTheGameIsOutFocus.LinkSetting(SettingsRoot.Sound.MuteAudioWhileTheGameIsOutFocus);
	}

	public void InitializeSettings()
	{
	}
}
