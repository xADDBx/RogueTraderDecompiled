using System;
using System.Collections.Generic;
using Core.Cheats;
using Kingmaker.Utility.Reporting.Base;

namespace CheatsCodeGen;

public static class AllCheats
{
	public static readonly List<CheatMethodInfoInternal> Methods = new List<CheatMethodInfoInternal>();

	public static readonly List<CheatPropertyInfoInternal> Properties = new List<CheatPropertyInfoInternal>
	{
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => ReportingCheats.IsDebugSaveFileChecksumEnabled),
			Setter = (Action<bool>)delegate(bool value)
			{
				ReportingCheats.IsDebugSaveFileChecksumEnabled = value;
			}
		}, "is_debug_savefilechecksum_enabled", "", "", ExecutionPolicy.PlayMode, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => ReportingCheats.IsDebugOpenReportLastIssueInBrowser),
			Setter = (Action<bool>)delegate(bool value)
			{
				ReportingCheats.IsDebugOpenReportLastIssueInBrowser = value;
			}
		}, "is_debug_open_report_last_issue_in_browser_enabled", "", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => ReportingCheats.IsNetReport),
			Setter = (Action<bool>)delegate(bool value)
			{
				ReportingCheats.IsNetReport = value;
			}
		}, "net_report", "", "", ExecutionPolicy.PlayMode, "bool")
	};

	public static readonly List<(ArgumentConverter.ConvertDelegate, int)> ArgConverters = new List<(ArgumentConverter.ConvertDelegate, int)>();

	public static readonly List<(ArgumentConverter.PreprocessDelegate, int)> ArgPreprocessors = new List<(ArgumentConverter.PreprocessDelegate, int)>();
}
