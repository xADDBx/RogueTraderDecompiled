using System;
using System.Linq;
using Kingmaker.Utility;

namespace Kingmaker.QA;

public class LongTermSpamDetectionStrategy : ISpamDetectionStrategy
{
	private readonly string m_Message;

	private int m_Count;

	private TimeSpan m_Time;

	private int m_Times;

	private readonly ReportingUtils.FixVersions m_TargetVersion;

	private readonly ReportingUtils.Severity m_Severity;

	public LongTermSpamDetectionStrategy(string message, ReportingUtils.Severity severity, ReportingUtils.FixVersions targetVersion, int count, TimeSpan time, int times)
	{
		m_Message = message;
		m_Count = count;
		m_Time = time;
		m_Times = times;
		m_TargetVersion = targetVersion;
		m_Severity = severity;
	}

	public (bool, SpamDetectionResult?) Check(RegistrationService<LogItem> registrationService)
	{
		LogItem logItem = registrationService.Get(0);
		if (logItem == null)
		{
			return default((bool, SpamDetectionResult));
		}
		DateTime dateTime = logItem.Time;
		bool flag = false;
		bool flag2 = true;
		DateTime dateTime2 = dateTime;
		for (int i = 0; i < m_Times; i++)
		{
			dateTime2 = dateTime - m_Time;
			bool flag3 = registrationService.GetInInterval(dateTime2, dateTime).Count() >= m_Count;
			flag2 = flag2 && flag3;
			flag = flag || flag2;
			if (!flag2)
			{
				break;
			}
			dateTime = dateTime2;
		}
		(bool, LongTermSpamDetectionResult) tuple = (flag2 ? (true, new LongTermSpamDetectionResult(m_Message, m_Severity, m_TargetVersion, registrationService.GetInInterval(dateTime2, logItem.Time))) : (flag ? (true, null) : default((bool, LongTermSpamDetectionResult))));
		return (tuple.Item1, tuple.Item2);
	}

	public void Set(int count, int intervalMs, int times)
	{
		m_Count = count;
		m_Time = TimeSpan.FromMilliseconds(intervalMs);
		m_Times = times;
	}
}
