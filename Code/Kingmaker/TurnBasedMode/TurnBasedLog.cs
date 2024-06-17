namespace Kingmaker.TurnBasedMode;

public static class TurnBasedLog
{
	public static void Log(string text)
	{
		PFLog.TBM.Log(text);
	}

	public static void LogError(string text)
	{
		PFLog.TBM.Error(text);
	}
}
