using System.Collections.Generic;

namespace Kingmaker.QA.Clockwork;

public class AttemptsCounter
{
	public static AttemptsCounter Instance = new AttemptsCounter();

	private int m_HistoryLength;

	private int m_Current;

	private List<KeyValuePair<string, int>> m_Targets;

	public bool Enabled = true;

	public List<KeyValuePair<string, int>> Targets => m_Targets;

	public int MaxAttempts { get; set; }

	public AttemptsCounter(int historyLength = 5, int maxAttempts = 10)
	{
		MaxAttempts = maxAttempts;
		m_HistoryLength = historyLength;
	}

	public void AddAttempt(string target)
	{
		int num = m_Targets.FindIndex((KeyValuePair<string, int> value) => value.Key == target);
		if (num == -1)
		{
			m_Targets[m_Current] = new KeyValuePair<string, int>(target, 0);
			m_Current++;
			m_Current %= m_HistoryLength;
		}
		else
		{
			KeyValuePair<string, int> value2 = new KeyValuePair<string, int>(target, m_Targets[num].Value + 1);
			m_Targets[num] = value2;
		}
	}

	public bool CheckTooManyAttempts(string target)
	{
		if (!Enabled)
		{
			return false;
		}
		int num = m_Targets.FindIndex((KeyValuePair<string, int> value) => value.Key == target);
		if (num == -1)
		{
			return false;
		}
		if (m_Targets[num].Value > MaxAttempts)
		{
			return true;
		}
		return false;
	}

	public void Reset()
	{
		m_Targets = new List<KeyValuePair<string, int>>(new KeyValuePair<string, int>[m_HistoryLength]);
	}
}
