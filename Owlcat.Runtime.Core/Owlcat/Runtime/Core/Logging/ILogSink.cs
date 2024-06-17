namespace Owlcat.Runtime.Core.Logging;

public interface ILogSink
{
	void Log(LogInfo logInfo);

	void Destroy();
}
