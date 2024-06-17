using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class SoundSettings
{
	public readonly SettingsEntityFloat VolumeMaster;

	public readonly SettingsEntityFloat VolumeVoices;

	public readonly SettingsEntityFloat VolumeVoicesCharacterInGame;

	public readonly SettingsEntityFloat VolumeVoicesNpcInGame;

	public readonly SettingsEntityFloat VolumeVoicesDialogues;

	public readonly SettingsEntityFloat VolumeMusic;

	public readonly SettingsEntityFloat VolumeSfx;

	public readonly SettingsEntityFloat VolumeAmbience;

	public readonly SettingsEntityFloat VolumeAbilities;

	public readonly SettingsEntityFloat VolumeRangedWeapons;

	public readonly SettingsEntityFloat VolumeMeleeWeapons;

	public readonly SettingsEntityFloat VolumeHitsLevel;

	public readonly SettingsEntityFloat VolumeUI;

	public readonly SettingsEntityEnum<VoiceAskFrequency> VoicedAskFrequency;

	public readonly SettingsEntityBool MuteAudioWhileTheGameIsOutFocus;

	public SoundSettings(ISettingsController settingsController, SoundSettingsDefaultValues defaultValues)
	{
		VolumeMaster = new SettingsEntityFloat(settingsController, "volume-master", defaultValues.VolumeMaster);
		VolumeVoices = new SettingsEntityFloat(settingsController, "volume-voices", defaultValues.VolumeVoices);
		VolumeVoicesCharacterInGame = new SettingsEntityFloat(settingsController, "volume-voices-character-in-game", defaultValues.VolumeVoicesCharacterInGame);
		VolumeVoicesNpcInGame = new SettingsEntityFloat(settingsController, "volume-voices-npc-in-game", defaultValues.VolumeVoicesNpcInGame);
		VolumeVoicesDialogues = new SettingsEntityFloat(settingsController, "volume-voices-dialogues", defaultValues.VolumeVoicesDialogues);
		VolumeMusic = new SettingsEntityFloat(settingsController, "volume-music", defaultValues.VolumeMusic);
		VolumeSfx = new SettingsEntityFloat(settingsController, "volume-sfx", defaultValues.VolumeSfx);
		VolumeAmbience = new SettingsEntityFloat(settingsController, "volume-ambience", defaultValues.VolumeAmbience);
		VolumeAbilities = new SettingsEntityFloat(settingsController, "volume-abilities", defaultValues.VolumeAbilities);
		VolumeRangedWeapons = new SettingsEntityFloat(settingsController, "volume-ranged-weapons", defaultValues.VolumeRangedWeapons);
		VolumeMeleeWeapons = new SettingsEntityFloat(settingsController, "volume-melee-weapons", defaultValues.VolumeMeleeWeapons);
		VolumeHitsLevel = new SettingsEntityFloat(settingsController, "volume-hits-level", defaultValues.VolumeHitsLevel);
		VolumeUI = new SettingsEntityFloat(settingsController, "volume-ui", defaultValues.VolumeUI);
		VoicedAskFrequency = new SettingsEntityEnum<VoiceAskFrequency>(settingsController, "voice-ask-frequency", defaultValues.VoicedAskFrequency);
		MuteAudioWhileTheGameIsOutFocus = new SettingsEntityBool(settingsController, "mute-audio-while-the-game-is-out-focus", defaultValues.MuteAudioWhileTheGameIsOutFocus);
	}
}
