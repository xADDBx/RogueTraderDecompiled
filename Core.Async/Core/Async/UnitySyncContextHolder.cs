using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.Async;

public static class UnitySyncContextHolder
{
	internal static int UnityThreadId { get; private set; }

	internal static SynchronizationContext UnitySynchronizationContext { get; private set; }

	public static TaskScheduler UnityTaskScheduler { get; private set; }

	public static TaskFactory UnityTaskFactory { get; private set; }

	public static bool IsInUnity => UnityThreadId == Thread.CurrentThread.ManagedThreadId;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Install()
	{
		UnityThreadId = Thread.CurrentThread.ManagedThreadId;
		UnitySynchronizationContext = SynchronizationContext.Current;
		UnityTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
		UnityTaskFactory = new TaskFactory(UnityTaskScheduler);
	}
}
