using System;

namespace Kingmaker.Settings;

[Serializable]
public class SoundSettingsDefaultValues
{
	public float VolumeMaster;

	public float VolumeVoices;

	public float VolumeVoicesCharacterInGame;

	public float VolumeVoicesNpcInGame;

	public float VolumeVoicesDialogues;

	public float VolumeMusic;

	public float VolumeSfx;

	public float VolumeAmbience;

	public float VolumeAbilities;

	public float VolumeRangedWeapons;

	public float VolumeMeleeWeapons;

	public float VolumeHitsLevel;

	public float VolumeUI;

	public VoiceAskFrequency VoicedAskFrequency;

	public bool MuteAudioWhileTheGameIsOutFocus;
}
