using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.QA;

public class QAModeExceptionReporterHelper
{
	[CanBeNull]
	private static QAModeExceptionReporterHelper s_Instance;

	[NotNull]
	public static QAModeExceptionReporterHelper Instance
	{
		get
		{
			Initialize();
			return s_Instance;
		}
	}

	public static void Initialize()
	{
		if (s_Instance == null)
		{
			s_Instance = new QAModeExceptionReporterHelper();
			QAModeExceptionEvents instance = QAModeExceptionEvents.Instance;
			instance.ShowErrorEvent = (Action<string, Exception, UnityEngine.Object>)Delegate.Combine(instance.ShowErrorEvent, new Action<string, Exception, UnityEngine.Object>(s_Instance.ShowErrorEvent));
		}
	}

	private void ShowErrorEvent(string message, Exception ex, UnityEngine.Object ctx)
	{
		QAModeExceptionReporter.MaybeShowError(message, ex, ctx);
	}
}
