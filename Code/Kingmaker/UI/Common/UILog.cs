using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.UI.Common;

public static class UILog
{
	private static readonly LogChannel UILogger = LogChannelFactory.GetOrCreate("UI");

	private static readonly LogChannel MVVMLogger = LogChannelFactory.GetOrCreate("MVVM");

	private static readonly LogChannel InputLayerLogger = LogChannelFactory.GetOrCreate("MVVM");

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
		UILogger.Log(text);
	}

	public static void ContextCreated(string name)
	{
	}

	public static void VMCreated(string name)
	{
	}

	public static void ViewBinded(string name)
	{
	}

	public static void ViewUnbinded(string name)
	{
	}

	public static void VMDisposed(string name)
	{
	}

	public static void ContextDisposed(string name)
	{
	}

	public static void Warning(string s)
	{
		MVVMLogger.Warning(s);
	}

	public static void SetBaseLayer(string layerContextName)
	{
		InputLayerLogger.Log("Set Base Layer: " + layerContextName + ".");
	}

	public static void PushLayer(string layerContextName)
	{
		InputLayerLogger.Log("Layer " + layerContextName + " pushed.");
	}

	public static void PopLayer(string layerContextName)
	{
		InputLayerLogger.Log("Layer " + layerContextName + " popped.");
	}

	public static void RemoveLayer(string layerContextName)
	{
		InputLayerLogger.Log("Layer " + layerContextName + " removed.");
	}
}
