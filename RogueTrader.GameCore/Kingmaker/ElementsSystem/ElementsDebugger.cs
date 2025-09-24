using System;
using Code.GameCore.ElementsSystem;
using ElementsSystem.Debug;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.ElementsSystem;

public class ElementsDebugger : ContextData<ElementsDebugger>
{
	public class LogSink : ILogSink
	{
		public void Log(LogInfo logInfo)
		{
			ContextData<ElementsDebugger>.Current?.Log(logInfo);
		}

		public void Destroy()
		{
		}
	}

	public delegate void EnterCallbackType(ElementsDebugger debugger);

	public delegate void ExitCallbackType(ElementsDebugger debugger);

	public delegate void LogCallbackType(ElementsDebugger debugger, [NotNull] LogInfo logInfo);

	public delegate void ClearExceptionCallbackType([CanBeNull] Element element, [NotNull] Exception exception);

	public static EnterCallbackType EnterCallback;

	public static ExitCallbackType ExitCallback;

	public static LogCallbackType LogCallback;

	public static ClearExceptionCallbackType ClearExceptionCallback;

	public int? Result { get; private set; }

	[CanBeNull]
	public Exception Exception { get; private set; }

	[CanBeNull]
	public ContextDebugData ContextDebugData { get; set; }

	public ElementsList List { get; private set; }

	public Element Element { get; private set; }

	public static bool Enabled { get; set; }

	public static bool IsContextDebugEnabled { get; }

	public static bool LogEnabled { get; set; }

	[CanBeNull]
	public static ElementsDebugger Scope([CanBeNull] ElementsList list, [CanBeNull] Element element = null)
	{
		if (!Enabled)
		{
			return null;
		}
		ElementsDebugger elementsDebugger = ContextData<ElementsDebugger>.Request();
		elementsDebugger.List = list;
		elementsDebugger.Element = element;
		EnterCallback?.Invoke(elementsDebugger);
		return elementsDebugger;
	}

	protected override void Reset()
	{
		ExitCallback?.Invoke(this);
		List = null;
		Element = null;
		Result = null;
		Exception = null;
	}

	private void Log(LogInfo logInfo)
	{
		if (Enabled)
		{
			LogCallback?.Invoke(this, logInfo);
		}
	}

	public void SetResult(int value)
	{
		Result = value;
	}

	public void SetException(Exception exception)
	{
		Exception = exception;
	}

	public static void ClearException(Element element, Exception exception)
	{
		ClearExceptionCallback?.Invoke(element, exception);
	}
}
