namespace Kingmaker.Sound;

public static class AudioMuteManager
{
	private static bool s_MusicMute;

	private static bool s_AllSoundMute;

	private static void SetNoneState()
	{
		AkSoundEngine.SetState("AudioStatus", "None");
	}

	private static void SetAllAudioMuteState()
	{
		AkSoundEngine.SetState("AudioStatus", "AllAudioMute");
	}

	private static void SetMusicMuteState()
	{
		AkSoundEngine.SetState("AudioStatus", "MusicMute");
	}

	public static void ToggleAllMute()
	{
		s_AllSoundMute = !s_AllSoundMute;
		UpdateState();
	}

	public static void ToggleMusicMute()
	{
		s_MusicMute = !s_MusicMute;
		UpdateState();
	}

	private static void UpdateState()
	{
		if (s_AllSoundMute)
		{
			SetAllAudioMuteState();
		}
		else if (s_MusicMute)
		{
			SetMusicMuteState();
		}
		else
		{
			SetNoneState();
		}
	}
}
