using Kingmaker.Utility.CommandLineArgs;

namespace Kingmaker.Utility.UnityExtensions;

public static class ApplicationFocusEvents
{
	private static bool s_Initialized;

	public static bool Disabled { get; private set; }

	public static bool DragDisabled { get; private set; }

	public static bool PingDisabled { get; private set; }

	public static bool DollRoomDisabled { get; private set; }

	public static bool TmpDisabled { get; private set; }

	public static bool CharacterDisabled { get; private set; }

	public static bool LocalMapDisabled { get; private set; }

	public static bool FowDisabled { get; private set; }

	public static bool DataPrivacyDisabled { get; private set; }

	public static bool EventBusDisabled { get; private set; }

	public static bool AutoPauseDisabled { get; private set; }

	public static bool CursorDisabled { get; private set; }

	public static bool KeyboardDisabled { get; private set; }

	public static bool SoundDisabled { get; private set; }

	public static void Initialize()
	{
		if (!s_Initialized)
		{
			s_Initialized = true;
			CommandLineArguments commandLineArguments = CommandLineArguments.Parse();
			Disabled = commandLineArguments.Contains("focus-disable");
			DragDisabled = Disabled || commandLineArguments.Contains("focus-disable-drag");
			PingDisabled = Disabled || commandLineArguments.Contains("focus-disable-ping");
			DollRoomDisabled = Disabled || commandLineArguments.Contains("focus-disable-dollroom");
			TmpDisabled = Disabled || commandLineArguments.Contains("focus-disable-tmp");
			CharacterDisabled = Disabled || commandLineArguments.Contains("focus-disable-character");
			LocalMapDisabled = Disabled || commandLineArguments.Contains("focus-disable-localmap");
			FowDisabled = Disabled || commandLineArguments.Contains("focus-disable-fow");
			DataPrivacyDisabled = Disabled || commandLineArguments.Contains("focus-disable-dataprivacy");
			EventBusDisabled = Disabled || commandLineArguments.Contains("focus-disable-eventbus");
			AutoPauseDisabled = Disabled || commandLineArguments.Contains("focus-disable-autopause");
			CursorDisabled = Disabled || commandLineArguments.Contains("focus-disable-cursor");
			KeyboardDisabled = Disabled || commandLineArguments.Contains("focus-disable-keyboard");
			SoundDisabled = Disabled || commandLineArguments.Contains("focus-disable-sound");
		}
	}
}
