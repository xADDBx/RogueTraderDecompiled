using System.Diagnostics;

namespace Kingmaker.PubSubSystem.Core;

public static class DetailedProfiler
{
	[Conditional("EVENT_BUS_PROFILE")]
	public static void BeginSample(string name)
	{
	}

	[Conditional("EVENT_BUS_PROFILE")]
	public static void EndSample()
	{
	}
}
