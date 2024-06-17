using System;

namespace Kingmaker.Controllers.Timer;

public class SimpleTimer : ITimer
{
	private readonly Action m_Callback;

	public TimeSpan TimerTime { get; }

	public SimpleTimer(Action callback, TimeSpan timerTime)
	{
		m_Callback = callback;
		TimerTime = timerTime;
	}

	public void RunCallback()
	{
		m_Callback?.Invoke();
	}
}
