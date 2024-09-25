using System;

namespace Kingmaker.Controllers.Timer;

public class TimerTime
{
	public TimeSpan Time { get; }

	public TimerTime(TimeSpan time)
	{
		Time = time;
	}
}
