using System.Text.RegularExpressions;
using Kingmaker.GameInfo;
using Kingmaker.Utility.Reporting.Base;
using UnityEngine;

namespace Kingmaker.Utility;

internal static class ReportBuildInfo
{
	private static readonly Regex BuildInfoRegex;

	public static string DateTime { get; private set; }

	public static string Revision { get; private set; }

	public static string Version { get; private set; }

	public static string Branch { get; private set; }

	static ReportBuildInfo()
	{
		BuildInfoRegex = new Regex("^(?<time>\\d{4}-[A-Za-z]{3}-\\d{2} \\d{2}:\\d{2}) (?<revision>[a-fA-F0-9]{40}) (?<version>\\S+) (?<branch>\\S+)$", RegexOptions.Compiled);
		DateTime = "";
		Revision = "";
		Version = "";
		Branch = "";
		if (Application.isEditor)
		{
			DateTime = "";
			Revision = ReportVersionManager.GetCommitOrRevision();
			Version = "Editor";
			Branch = "";
		}
		Match match = BuildInfoRegex.Match(GameVersion.Revision);
		if (match.Success)
		{
			DateTime = match.Groups["time"].Value;
			Revision = match.Groups["revision"].Value;
			Version = match.Groups["version"].Value;
			Branch = match.Groups["branch"].Value;
		}
	}
}
