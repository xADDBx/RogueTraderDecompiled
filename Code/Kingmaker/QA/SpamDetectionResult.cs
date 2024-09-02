using System.Collections.Generic;

namespace Kingmaker.QA;

public class SpamDetectionResult
{
	public readonly IEnumerable<LogItem> Items;

	public string Message { get; }

	public string TargetVersion { get; }

	public string Severity { get; }

	public SpamDetectionResult(string message, string severity, string targetVersion, IEnumerable<LogItem> items)
	{
		Message = message;
		Severity = severity;
		TargetVersion = targetVersion;
		Items = items;
	}
}
