using UnityEngine;
using UnityEngine.Assertions;

namespace Kingmaker.QA;

public static class AssertionsHandler
{
	private const string ReportMessage = "A critical bug detected, please report to programmers!";

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void Init()
	{
		Assert.raiseExceptions = false;
		Application.logMessageReceived += LogCallback;
	}

	private static void LogCallback(string condition, string stacktrace, LogType type)
	{
		if (type == LogType.Assert)
		{
			QAModeExceptionEvents.Instance.MaybeShowError("A critical bug detected, please report to programmers!\n\n" + condition + "\n" + stacktrace);
		}
	}
}
