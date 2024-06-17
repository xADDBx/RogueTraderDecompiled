using Kingmaker.Blueprints;

namespace Kingmaker.Utility;

public class ReportingUtilsSourceHolder
{
	private static ReportingUtilsSourceHolder s_Instance;

	public static ReportingUtilsSourceHolder Instance => s_Instance ?? (s_Instance = new ReportingUtilsSourceHolder());

	public BlueprintScriptableObject ExceptionSource { get; set; }
}
