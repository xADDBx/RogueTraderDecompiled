using System.IO;
using Kingmaker.Logging.Configuration.Platforms;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker;

public class EditorLoggingConfiguration : ILoggingConfiguration
{
	public void Configure()
	{
		string logFilePath = Path.Combine(Application.dataPath, "..");
		Owlcat.Runtime.Core.Logging.Logger.Instance.Enabled = true;
		Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(LogSinkFactory.CreateHistory());
		Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(LogSinkFactory.CreateFull(logFilePath, "EditorLogFull.txt"));
		Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(LogSinkFactory.CreateShort(logFilePath, "EditorLog.txt"));
		Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(LogSinkFactory.CreateElementsDebugger());
	}
}
