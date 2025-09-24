using System.Linq;
using Kingmaker.GameInfo;

namespace Kingmaker.QA.Arbiter.GameCore;

internal class GameCoreArbiterEnvironment : IArbiterEnvironment
{
	public static bool FatalError;

	public string Version => GameVersion.GetVersion();

	public bool IsAvailable
	{
		get
		{
			if (FatalError)
			{
				return ArbiterIntegration.IsMainMenuActive();
			}
			return true;
		}
	}

	public bool IsLoggingEnabled => LoggingConfiguration.IsLoggingEnabled;

	public string Branch => GameVersion.Revision.Split(' ').Last();

	public string Revision
	{
		get
		{
			string[] array = GameVersion.Revision.Split(' ');
			if (array.Length >= 3)
			{
				return array[2];
			}
			return array[0];
		}
	}

	public string ProjectAlias => "WH";
}
