using UnityEngine;

namespace Kingmaker;

public static class LoggingConfigurationProvider
{
	public static ILoggingConfiguration PickConfiguration()
	{
		switch (Application.platform)
		{
		case RuntimePlatform.PS4:
			return new ConsoleLoggingConfiguration(enableLogsForwardingToUnity: false);
		case RuntimePlatform.PS5:
			return new ConsoleLoggingConfiguration(enableLogsForwardingToUnity: false);
		case RuntimePlatform.GameCoreXboxSeries:
			return new ConsoleLoggingConfiguration(enableLogsForwardingToUnity: true);
		default:
			if (Application.isEditor)
			{
				return new EditorLoggingConfiguration();
			}
			return new DefaultLoggingConfiguration();
		}
	}
}
