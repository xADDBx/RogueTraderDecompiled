using System;

namespace Kingmaker.Controllers.Timer;

public interface ITimer
{
	TimeSpan TimerTime { get; }

	void RunCallback();
}
