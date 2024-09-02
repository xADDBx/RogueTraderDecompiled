using Core.Cheats;

namespace Kingmaker.Utility.Reporting.Base;

public static class ReportingCheats
{
	[Cheat(Name = "is_debug_saveFileChecksum_enabled", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static bool IsDebugSaveFileChecksumEnabled { get; set; } = true;


	[Cheat(Name = "is_debug_open_report_last_issue_in_browser_enabled", ExecutionPolicy = ExecutionPolicy.All)]
	public static bool IsDebugOpenReportLastIssueInBrowser { get; set; } = true;


	[Cheat(Name = "net_report", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static bool IsNetReport { get; set; } = true;

}
