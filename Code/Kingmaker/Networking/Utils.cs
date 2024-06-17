using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kingmaker.Networking;

public static class Utils
{
	public static Action CallbackToTask(out Task task)
	{
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		Action result = delegate
		{
			tcs.SetResult(result: true);
		};
		task = tcs.Task;
		return result;
	}

	public static Action<T> CallbackToTask<T>(out Task<T> task)
	{
		TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
		Action<T> result2 = delegate(T result)
		{
			tcs.SetResult(result);
		};
		task = tcs.Task;
		return result2;
	}

	public static async Task OrCancelledBy(this Task task, CancellationToken cancellationToken)
	{
		await (await Task.WhenAny(task, Task.Delay(-1, cancellationToken)));
	}
}
