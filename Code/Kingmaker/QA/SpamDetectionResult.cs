using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility;

namespace Kingmaker.QA;

public class SpamDetectionResult
{
	public readonly IEnumerable<LogItem> Items;

	public string Message { get; }

	public ReportingUtils.FixVersions TargetVersion { get; }

	public ReportingUtils.Severity Severity { get; }

	public SpamDetectionResult(string message, ReportingUtils.Severity severity, ReportingUtils.FixVersions targetVersion, IEnumerable<LogItem> items)
	{
		Message = message;
		Severity = severity;
		TargetVersion = targetVersion;
		Items = items.ToList();
	}
}
