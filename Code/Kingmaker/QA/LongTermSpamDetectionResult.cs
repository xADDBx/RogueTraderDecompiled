using System.Collections.Generic;

namespace Kingmaker.QA;

public class LongTermSpamDetectionResult : SpamDetectionResult
{
	public LongTermSpamDetectionResult(string message, string severity, string targetVersion, IEnumerable<LogItem> items)
		: base(message, severity, targetVersion, items)
	{
	}
}
