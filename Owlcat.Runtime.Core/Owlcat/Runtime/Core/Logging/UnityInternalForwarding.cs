using System;
using System.Reflection;
using UnityEngine;

namespace Owlcat.Runtime.Core.Logging;

public static class UnityInternalForwarding
{
	public delegate void InternalLog(LogType level, LogOption options, string msg, UnityEngine.Object obj);

	public delegate void InternalLogException(Exception ex, UnityEngine.Object obj);

	public static readonly InternalLog Log;

	public static readonly InternalLogException LogException;

	static UnityInternalForwarding()
	{
		Type type = typeof(ILogHandler).Assembly.GetType("UnityEngine.DebugLogHandler");
		MethodInfo methodInfo = type?.GetMethod("Internal_Log", BindingFlags.Static | BindingFlags.NonPublic);
		MethodInfo obj = type?.GetMethod("Internal_LogException", BindingFlags.Static | BindingFlags.NonPublic);
		Log = ((InternalLog)(methodInfo?.CreateDelegate(typeof(InternalLog)))) ?? ((InternalLog)delegate
		{
		});
		LogException = ((InternalLogException)(obj?.CreateDelegate(typeof(InternalLogException)))) ?? ((InternalLogException)delegate
		{
		});
	}
}
