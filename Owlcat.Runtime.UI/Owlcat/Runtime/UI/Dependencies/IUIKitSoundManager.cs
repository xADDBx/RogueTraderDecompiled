namespace Owlcat.Runtime.UI.Dependencies;

public interface IUIKitSoundManager
{
	void PlayHoverSound(int soundType = -1);

	void PlayButtonClickSound(int soundType = -1);

	void PlayConsoleHintClickSound();

	void PlayConsoleHintHoldSoundStart();

	void PlayConsoleHintHoldSoundStop();

	void PlayConsoleHintHoldSoundSetRtpcValue(float value);
}
