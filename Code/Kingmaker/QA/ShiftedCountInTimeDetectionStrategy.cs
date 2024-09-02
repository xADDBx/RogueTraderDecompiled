using System;

namespace Kingmaker.QA;

public class ShiftedCountInTimeDetectionStrategy : ISpamDetectionStrategy
{
	private readonly string m_Message;

	private int m_Count;

	private TimeSpan m_Time;

	private readonly int m_Shift;

	private readonly TimeSpan m_Treshold;

	private readonly string m_Severity;

	private readonly string m_TargetVersion;

	public ShiftedCountInTimeDetectionStrategy(string message, string severity, string targetVersion, int count, TimeSpan time, int shift, TimeSpan treshold)
	{
		m_Message = message;
		m_Count = count;
		m_Time = time;
		m_Shift = shift;
		m_Treshold = treshold;
		m_Severity = severity;
		m_TargetVersion = targetVersion;
	}

	public (bool, SpamDetectionResult?) Check(RegistrationService<LogItem> registrationService)
	{
		LogItem logItem = registrationService.Get(-m_Shift);
		if (logItem == null)
		{
			return default((bool, SpamDetectionResult));
		}
		if (m_Shift != 0)
		{
			LogItem logItem2 = registrationService.Get(0);
			if (logItem2 == null)
			{
				return default((bool, SpamDetectionResult));
			}
			if (logItem2.Time - logItem.Time > m_Treshold)
			{
				return default((bool, SpamDetectionResult));
			}
		}
		LogItem logItem3 = registrationService.Get(-(m_Shift + m_Count - 1));
		if (logItem3 == null)
		{
			return default((bool, SpamDetectionResult));
		}
		if (logItem.Time - logItem3.Time >= m_Time)
		{
			return (true, null);
		}
		return (true, new SpamDetectionResult(m_Message, m_Severity, m_TargetVersion, registrationService.GetInInterval(logItem3.Time, logItem.Time)));
	}

	public void Set(int count, int intervalMs)
	{
		m_Count = count;
		m_Time = TimeSpan.FromMilliseconds(intervalMs);
	}
}
