using System;
using System.Threading.Tasks;

namespace Core.Async;

public static class UnityThreadHelper
{
	public static async Task Run(Action action)
	{
		await Awaiters.UnityThread;
		action();
	}

	public static async Task<T> Run<T>(Func<T> action)
	{
		await Awaiters.UnityThread;
		return action();
	}

	public static async Task Run(Func<Task> action)
	{
		await Awaiters.UnityThread;
		await action();
	}

	public static async Task<T> Run<T>(Func<Task<T>> action)
	{
		await Awaiters.UnityThread;
		return await action();
	}
}
