using System;
using Kingmaker.Utility;

namespace Kingmaker.QA;

public class CountInTimeDetectionStrategy : ISpamDetectionStrategy
{
	private readonly string m_Message;

	private int _count;

	private TimeSpan _time;

	private readonly ReportingUtils.Severity m_Severity;

	private readonly ReportingUtils.FixVersions m_TargetVersion;

	public CountInTimeDetectionStrategy(string message, ReportingUtils.Severity severity, ReportingUtils.FixVersions targetVersion, int count, TimeSpan time)
	{
		m_Message = message;
		_count = count;
		_time = time;
		m_TargetVersion = targetVersion;
		m_Severity = severity;
	}

	public (bool, SpamDetectionResult?) Check(RegistrationService<LogItem> registrationService)
	{
		LogItem logItem = registrationService.Get(0);
		LogItem logItem2 = registrationService.Get(-(_count - 1));
		if (logItem == null || logItem2 == null)
		{
			return default((bool, SpamDetectionResult));
		}
		if (logItem.Time - logItem2.Time >= _time)
		{
			return default((bool, SpamDetectionResult));
		}
		return (true, new SpamDetectionResult(m_Message, m_Severity, m_TargetVersion, registrationService.GetInInterval(logItem2.Time, logItem.Time)));
	}

	public void Set(int count, int intervalMs)
	{
		_count = count;
		_time = TimeSpan.FromMilliseconds(intervalMs);
	}
}
