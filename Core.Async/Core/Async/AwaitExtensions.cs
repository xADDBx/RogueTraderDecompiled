using System.Threading.Tasks;
using UnityEngine;

namespace Core.Async;

public static class AwaitExtensions
{
	public static async Task AwaitAsyncOp(this AsyncOperation op)
	{
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		await UnityThreadHelper.Run(delegate
		{
			op.completed += delegate
			{
				tcs.SetResult(result: true);
			};
		});
		await tcs.Task;
	}
}
