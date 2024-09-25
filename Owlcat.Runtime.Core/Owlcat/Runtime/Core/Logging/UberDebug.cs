using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.Runtime.Core.Logging;

public static class UberDebug
{
	[StackTraceIgnore]
	[StringFormatMethod("message")]
	public static void Log(string message, params object[] par)
	{
		LogChannel.Default.Log(message, par);
	}

	[StackTraceIgnore]
	[StringFormatMethod("message")]
	public static void LogError(Object context, string message, params object[] par)
	{
		LogChannel.Default.Error(context, message, par);
	}

	[StackTraceIgnore]
	[StringFormatMethod("message")]
	public static void LogError(string message, params object[] par)
	{
		LogChannel.Default.Error(message, par);
	}
}
