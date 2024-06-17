using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.QA;

public class QAModeExceptionEvents
{
	[CanBeNull]
	private static QAModeExceptionEvents s_Instance;

	public Action<string, Exception, UnityEngine.Object> ShowErrorEvent;

	[NotNull]
	public static QAModeExceptionEvents Instance => s_Instance ?? (s_Instance = new QAModeExceptionEvents());

	public void MaybeShowError(string message, Exception ex = null, UnityEngine.Object ctx = null)
	{
		ShowErrorEvent?.Invoke(message, ex, ctx);
	}
}
