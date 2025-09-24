using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker;

public class Switch2LoggingConfiguration : ILoggingConfiguration
{
	public void Configure()
	{
		Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
		Owlcat.Runtime.Core.Logging.Logger.Instance.Enabled = LoggingConfiguration.IsLoggingEnabled;
		UnityInternalUberLogSink.Enabled = true;
		if (!Owlcat.Runtime.Core.Logging.Logger.Instance.Enabled)
		{
			Debug.Log("Storing logs is not enabled");
		}
	}
}
