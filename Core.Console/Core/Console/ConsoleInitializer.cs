using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Core.Console;

public static class ConsoleInitializer
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void Init()
	{
		ConsoleLogSink consoleLogSink = new ConsoleLogSink();
		Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(consoleLogSink);
		ConsoleLogSink.Instance = consoleLogSink;
	}
}
