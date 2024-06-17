namespace Kingmaker.Settings;

public class SoundSettingsController
{
	private SoundSettings m_Settings;

	public SoundSettingsController()
	{
		m_Settings = SettingsRoot.Sound;
		SettingsToRealMasterVolume();
		SettingsToRealMusicVolume();
		m_Settings.VolumeMaster.OnTempValueChanged += delegate
		{
			SettingsToRealMasterVolume();
		};
		m_Settings.VolumeMusic.OnTempValueChanged += delegate
		{
			SettingsToRealMusicVolume();
		};
		SettingsVoicesVolume(m_Settings.VolumeVoices);
		m_Settings.VolumeVoices.OnTempValueChanged += SettingsVoicesVolume;
		m_Settings.VolumeVoicesCharacterInGame.ResetToDefault();
		SettingsVoicesCharacterInGameVolume(m_Settings.VolumeVoicesCharacterInGame);
		m_Settings.VolumeVoicesCharacterInGame.OnTempValueChanged += SettingsVoicesCharacterInGameVolume;
		m_Settings.VolumeVoicesNpcInGame.ResetToDefault();
		SettingsVoicesNpcInGameVolume(m_Settings.VolumeVoicesNpcInGame);
		m_Settings.VolumeVoicesNpcInGame.OnTempValueChanged += SettingsVoicesNpcInGameVolume;
		m_Settings.VolumeVoicesDialogues.ResetToDefault();
		SettingsVoicesDialoguesVolume(m_Settings.VolumeVoicesDialogues);
		m_Settings.VolumeVoicesDialogues.OnTempValueChanged += SettingsVoicesDialoguesVolume;
		SettingsSfxVolume(m_Settings.VolumeSfx);
		m_Settings.VolumeSfx.OnTempValueChanged += SettingsSfxVolume;
		m_Settings.VolumeAmbience.ResetToDefault();
		SettingsAmbienceVolume(m_Settings.VolumeAmbience);
		m_Settings.VolumeAmbience.OnTempValueChanged += SettingsAmbienceVolume;
		m_Settings.VolumeAbilities.ResetToDefault();
		SettingsAbilitiesVolume(m_Settings.VolumeAbilities);
		m_Settings.VolumeAbilities.OnTempValueChanged += SettingsAbilitiesVolume;
		m_Settings.VolumeRangedWeapons.ResetToDefault();
		SettingsRangedWeaponsVolume(m_Settings.VolumeRangedWeapons);
		m_Settings.VolumeRangedWeapons.OnTempValueChanged += SettingsRangedWeaponsVolume;
		m_Settings.VolumeMeleeWeapons.ResetToDefault();
		SettingsMeleeWeaponsVolume(m_Settings.VolumeMeleeWeapons);
		m_Settings.VolumeMeleeWeapons.OnTempValueChanged += SettingsMeleeWeaponsVolume;
		m_Settings.VolumeHitsLevel.ResetToDefault();
		SettingsHitsLevelVolume(m_Settings.VolumeHitsLevel);
		m_Settings.VolumeHitsLevel.OnTempValueChanged += SettingsHitsLevelVolume;
		m_Settings.VolumeUI.ResetToDefault();
		SettingsUIVolume(m_Settings.VolumeUI);
		m_Settings.VolumeUI.OnTempValueChanged += SettingsUIVolume;
	}

	private void SettingsToRealMasterVolume()
	{
		float tempValue = m_Settings.VolumeMaster.GetTempValue();
		AkSoundEngine.SetRTPCValue("AudioLevel", tempValue, null, 0);
	}

	private void SettingsToRealMusicVolume()
	{
		float tempValue = m_Settings.VolumeMusic.GetTempValue();
		AkSoundEngine.SetRTPCValue("MusicLevel", tempValue, null, 0);
	}

	private void SettingsVoicesVolume(float v)
	{
		AkSoundEngine.SetRTPCValue("VoiceLevel", v, null, 0);
	}

	private void SettingsVoicesCharacterInGameVolume(float v)
	{
		AkSoundEngine.SetRTPCValue("CharactersLevel", v, null, 0);
	}

	private void SettingsVoicesNpcInGameVolume(float v)
	{
		AkSoundEngine.SetRTPCValue("NPCLevel", v, null, 0);
	}

	private void SettingsVoicesDialoguesVolume(float v)
	{
		AkSoundEngine.SetRTPCValue("DialogueLevel", v, null, 0);
	}

	private void SettingsSfxVolume(float v)
	{
		AkSoundEngine.SetRTPCValue("SFXLevel", v, null, 0);
	}

	private void SettingsAmbienceVolume(float v)
	{
		AkSoundEngine.SetRTPCValue("AmbienceLevel", v, null, 0);
	}

	private void SettingsAbilitiesVolume(float v)
	{
		AkSoundEngine.SetRTPCValue("AbilitiesLevel", v, null, 0);
	}

	private void SettingsRangedWeaponsVolume(float v)
	{
		AkSoundEngine.SetRTPCValue("RWeaponsLevel", v, null, 0);
	}

	private void SettingsMeleeWeaponsVolume(float v)
	{
		AkSoundEngine.SetRTPCValue("MWeaponsLevel", v, null, 0);
	}

	private void SettingsHitsLevelVolume(float v)
	{
		AkSoundEngine.SetRTPCValue("HitsLevel", v, null, 0);
	}

	private void SettingsUIVolume(float v)
	{
		AkSoundEngine.SetRTPCValue("UILevel", v, null, 0);
	}
}
