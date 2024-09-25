using UnityEngine;

namespace Owlcat.QA;

public static class QALogger
{
	private static ILogger m_Logger;

	private static ILogger Logger => m_Logger ?? Debug.unityLogger;

	public static void SetLogger(ILogger logger)
	{
		m_Logger = logger;
	}

	public static void Log(object message)
	{
		Logger.Log(LogType.Log, message);
	}

	public static void Log(Object context, object message)
	{
		Logger.Log(LogType.Log, message, context);
	}

	public static void Log(string tag, object message, Object context)
	{
		Logger.Log(LogType.Log, tag, message, context);
	}

	public static void Warning(object message)
	{
		Logger.Log(LogType.Warning, message);
	}

	public static void Warning(Object context, object message)
	{
		Logger.Log(LogType.Warning, message, context);
	}

	public static void Warning(string tag, object message, Object context)
	{
		Logger.Log(LogType.Warning, tag, message, context);
	}

	public static void Error(object message)
	{
		Logger.Log(LogType.Error, message);
	}

	public static void Error(Object context, object message)
	{
		Logger.Log(LogType.Error, message, context);
	}

	public static void Error(string tag, object message, Object context)
	{
		Logger.Log(LogType.Error, tag, message, context);
	}

	public static void Exception(object message)
	{
		Logger.Log(LogType.Exception, message);
	}

	public static void Exception(Object context, object message)
	{
		Logger.Log(LogType.Exception, message, context);
	}

	public static void Exception(string tag, object message, Object context)
	{
		Logger.Log(LogType.Exception, tag, message, context);
	}
}
