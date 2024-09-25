using Core.Cheats;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kingmaker.Utility;

public static class MemoryUsageHelper
{
	public class MemoryStatsProvider
	{
		private ProfilerRecorder s_SystemMemoryRecorder = new ProfilerRecorder(ProfilerCategory.Memory, "System Used Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);

		public long SystemMemoryLimit => (long)SystemInfo.systemMemorySize * 1024L * 1024;

		public long SystemMemoryUsed
		{
			get
			{
				if (!s_SystemMemoryRecorder.Valid)
				{
					return 0L;
				}
				return s_SystemMemoryRecorder.LastValue;
			}
		}

		public long SystemMemoryUsedPeak => 0L;
	}

	public static readonly MemoryStatsProvider Stats = new MemoryStatsProvider();

	[Cheat(Name = "memory_stats_dump")]
	public static void DumpMemoryStats()
	{
		MemoryStatsProvider stats = Stats;
		PFLog.Default.Log("Memory Statistics:\r\nGFX: " + MemString(Profiler.GetAllocatedMemoryForGraphicsDriver()) + "\r\nNative: " + MemString(Profiler.GetTotalAllocatedMemoryLong()) + " allocated, " + MemString(Profiler.GetTotalReservedMemoryLong()) + " reserved\r\nScript: " + MemString(Profiler.GetMonoUsedSizeLong()) + " used, " + MemString(Profiler.GetMonoHeapSizeLong()) + " heap\r\nSystem: " + MemString(stats.SystemMemoryUsed) + " / " + MemString(stats.SystemMemoryLimit) + " (peak: " + MemString(stats.SystemMemoryUsedPeak) + ")\r\n");
		static string MemString(long bytes)
		{
			return $"{(float)bytes / 1024f / 1024f:#.0} MB";
		}
	}
}
