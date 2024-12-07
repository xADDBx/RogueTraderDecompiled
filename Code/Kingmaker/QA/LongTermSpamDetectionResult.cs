using System.Collections.Generic;
using Kingmaker.Utility;

namespace Kingmaker.QA;

public class LongTermSpamDetectionResult : SpamDetectionResult
{
	public LongTermSpamDetectionResult(string message, ReportingUtils.Severity severity, ReportingUtils.FixVersions targetVersion, IEnumerable<LogItem> items)
		: base(message, severity, targetVersion, items)
	{
	}
}
