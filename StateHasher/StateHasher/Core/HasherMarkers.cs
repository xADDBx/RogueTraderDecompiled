using System.Diagnostics;
using Unity.Profiling;
using UnityEngine.Scripting;

namespace StateHasher.Core;

public static class HasherMarkers
{
	private static ProfilerMarker _blittableTypes = new ProfilerMarker("StateHasher.BlittableTypes");

	[Preserve]
	[Conditional("STATE_HASHER_PROFILE")]
	[IgnoredByDeepProfiler]
	public static void BeginGetter(string name)
	{
	}

	[Preserve]
	[Conditional("STATE_HASHER_PROFILE")]
	[IgnoredByDeepProfiler]
	public static void EndGetter()
	{
	}

	[Preserve]
	[Conditional("STATE_HASHER_PROFILE")]
	[IgnoredByDeepProfiler]
	public static void BeginBlittable()
	{
	}

	[Preserve]
	[Conditional("STATE_HASHER_PROFILE")]
	[IgnoredByDeepProfiler]
	public static void EndBlittable()
	{
	}
}
