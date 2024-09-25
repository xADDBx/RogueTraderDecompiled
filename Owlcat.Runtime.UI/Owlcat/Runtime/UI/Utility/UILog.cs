using Owlcat.Runtime.UI.Dependencies;

namespace Owlcat.Runtime.UI.Utility;

public static class UILog
{
	private static string ContextNameColor => "#3c5243";

	private static string ContextCreatedColor => "#00b33a";

	private static string ContextDisposedColor => "#b34e00";

	private static string VMNameColor => "#98a2b8";

	private static string VMCreatedColor => "#00b36d";

	private static string VMDisposedColor => "#b37000";

	private static string ViewBindedColor => "#00b3b3";

	private static string ViewDisposedColor => "#b39b00";

	private static string LayerPushedColor => "#00b3b3";

	private static string LayerPopedColor => "#b39b00";

	private static string LayerRemovedColor => "#b36e00";

	private static string WarningColor => "#db0000";

	private static string InputLayerName => "#ffffff";

	public static void Log(string text)
	{
		UIKitLogger.Log(text);
	}

	public static void MVVMLog(string text)
	{
		UIKitLogger.Log(text);
	}

	public static void ContextCreated(string name)
	{
		UIKitLogger.Log("Context " + name + " created");
	}

	public static void VMCreated(string name)
	{
		UIKitLogger.Log("View model " + name + " created.");
	}

	public static void ViewBinded(string name)
	{
		UIKitLogger.Log("View model " + name + " binded.");
	}

	public static void ViewUnbinded(string name)
	{
		UIKitLogger.Log("View model " + name + " unbinded.");
	}

	public static void VMDisposed(string name)
	{
		UIKitLogger.Log("View model " + name + " disposed.");
	}

	public static void ContextDisposed(string name)
	{
		UIKitLogger.Log("Context " + name + " disposed.");
	}

	public static void Warning(string s)
	{
		UIKitLogger.Log(s);
	}

	public static void SetBugReportLayer(string layerContextName)
	{
		UIKitLogger.Log("Set BugReport Layer: " + layerContextName + ".");
	}

	public static void SetBaseLayer(string layerContextName)
	{
		UIKitLogger.Log("Set Base Layer: " + layerContextName + ".");
	}

	public static void SetOverlayLayer(string layerContextName)
	{
		UIKitLogger.Log("Set Overlay Layer: " + layerContextName + ".");
	}

	public static void PushLayer(string layerContextName)
	{
		UIKitLogger.Log("Layer " + layerContextName + " pushed.");
	}

	public static void PopLayer(string layerContextName)
	{
		UIKitLogger.Log("Layer " + layerContextName + " popped.");
	}

	public static void RemoveLayer(string layerContextName)
	{
		UIKitLogger.Log("Layer " + layerContextName + " removed.");
	}
}
