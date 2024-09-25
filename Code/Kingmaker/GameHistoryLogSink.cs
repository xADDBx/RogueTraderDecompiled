using Owlcat.Runtime.Core.Logging;

namespace Kingmaker;

public class GameHistoryLogSink : ILogSink
{
	private readonly GameHistoryFile m_File;

	public GameHistoryLogSink(GameHistoryFile file)
	{
		m_File = file;
	}

	public void Log(LogInfo logInfo)
	{
		if (!IsHistorySinkDisabled(logInfo))
		{
			m_File.Append(logInfo.Message);
		}
	}

	private static bool IsHistorySinkDisabled(LogInfo logInfo)
	{
		int? num = logInfo.Channel?.SinkBitmap;
		if (num.HasValue)
		{
			return (num & 1) == 0;
		}
		return true;
	}

	public void Destroy()
	{
	}
}
