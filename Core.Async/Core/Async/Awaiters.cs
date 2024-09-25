namespace Core.Async;

public static class Awaiters
{
	public static readonly UnityThreadAwaitable UnityThread;

	public static readonly ThreadPoolRedirector ThreadPool;

	public static readonly ThreadPoolRedirectorForceYield ThreadPoolYield;
}
