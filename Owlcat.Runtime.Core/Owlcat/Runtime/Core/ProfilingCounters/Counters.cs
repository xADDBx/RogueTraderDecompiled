using JetBrains.Annotations;

namespace Owlcat.Runtime.Core.ProfilingCounters;

public class Counters
{
	[CanBeNull]
	public static readonly Counter Render;

	[CanBeNull]
	public static readonly Counter PBD;

	[CanBeNull]
	public static readonly Counter IK;

	static Counters()
	{
		Render = new Counter("Render", 10.0);
		PBD = new Counter("PBD", 1.5);
		IK = new Counter("IK", 1.0);
	}
}
