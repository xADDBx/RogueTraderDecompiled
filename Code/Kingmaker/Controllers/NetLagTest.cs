using System;

namespace Kingmaker.Controllers;

public static class NetLagTest
{
	public static bool Process(bool commandsReady, int tickIndex)
	{
		return commandsReady;
	}

	public static TimeSpan Process(TimeSpan deltaTime)
	{
		return deltaTime;
	}
}
