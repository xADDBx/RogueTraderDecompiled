using UnityEngine;

namespace Owlcat.Runtime.Core.Logging;

public static class LogLevelConverter
{
	public static LogType ToUnity(this LogSeverity level)
	{
		return level switch
		{
			LogSeverity.Error => LogType.Error, 
			LogSeverity.Warning => LogType.Warning, 
			LogSeverity.Message => LogType.Log, 
			_ => LogType.Log, 
		};
	}

	public static LogSeverity FromUnity(this LogType type)
	{
		return type switch
		{
			LogType.Exception => LogSeverity.Error, 
			LogType.Assert => LogSeverity.Error, 
			LogType.Error => LogSeverity.Error, 
			LogType.Warning => LogSeverity.Warning, 
			LogType.Log => LogSeverity.Message, 
			_ => LogSeverity.Message, 
		};
	}
}
