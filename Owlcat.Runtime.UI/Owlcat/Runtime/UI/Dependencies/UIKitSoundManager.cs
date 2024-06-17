namespace Owlcat.Runtime.UI.Dependencies;

public static class UIKitSoundManager
{
	private static IUIKitSoundManager s_SoundManager;

	public static void SetSoundManager(IUIKitSoundManager soundManager)
	{
		s_SoundManager = soundManager;
	}

	public static void PlayHoverSound(int soundType = -1)
	{
		s_SoundManager?.PlayHoverSound(soundType);
	}

	public static void PlayButtonClickSound(int soundType = -1)
	{
		s_SoundManager?.PlayButtonClickSound(soundType);
	}

	public static void PlayConsoleHintClickSound()
	{
		s_SoundManager?.PlayConsoleHintClickSound();
	}

	public static void PlayConsoleHintHoldSoundStart()
	{
		s_SoundManager?.PlayConsoleHintHoldSoundStart();
	}

	public static void PlayConsoleHintHoldSoundStop()
	{
		s_SoundManager?.PlayConsoleHintHoldSoundStop();
	}

	public static void PlayConsoleHintHoldSoundSetRtpcValue(float value)
	{
		s_SoundManager?.PlayConsoleHintHoldSoundSetRtpcValue(value);
	}
}
