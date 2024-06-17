using StateHasher.Core;

namespace Kingmaker.StateHasher;

public class PFHasherLogger : IHasherLogger
{
	public void Log(string message)
	{
		PFLog.Net.Log(message);
	}

	public void Error(string message)
	{
		PFLog.Net.Error(message);
	}
}
