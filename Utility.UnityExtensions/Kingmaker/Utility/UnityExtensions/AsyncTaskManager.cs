using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Utility.UnityExtensions;

public static class AsyncTaskManager
{
	public static void RunAsyncTask<T>(Func<T> asyncFunc, [CanBeNull] Action<T> callback = null)
	{
		CoroutineRunner.Start(WaitForAsyncTask(asyncFunc, callback));
	}

	public static void RunPoolOfAsyncTasks<T>(IEnumerable<Func<T>> asyncFuncPool, [CanBeNull] Action<List<T>> callback = null)
	{
		CoroutineRunner.Start(WaitForAsyncTaskPool(asyncFuncPool, callback));
	}

	private static IEnumerator WaitForAsyncTask<T>(Func<T> asyncFunc, [CanBeNull] Action<T> callback)
	{
		Task<T> task = new Task<T>(asyncFunc);
		task.Start();
		while (!task.IsCompleted && task.Status != TaskStatus.Faulted)
		{
			yield return null;
		}
		if (task.Status == TaskStatus.Faulted && task.Exception != null)
		{
			foreach (Exception innerException in task.Exception.InnerExceptions)
			{
				PFLog.Default.Error(innerException.Message);
			}
			callback?.Invoke(default(T));
		}
		else
		{
			callback?.Invoke(task.Result);
		}
	}

	private static IEnumerator WaitForAsyncTaskPool<T>(IEnumerable<Func<T>> asyncFuncPool, [CanBeNull] Action<List<T>> callback = null)
	{
		List<Task<T>> tasks = new List<Task<T>>();
		foreach (Func<T> item in asyncFuncPool)
		{
			Task<T> task = new Task<T>(item);
			task.Start();
			tasks.Add(task);
		}
		while (tasks.Any((Task<T> t) => !t.IsCompleted && t.Status != TaskStatus.Faulted))
		{
			yield return null;
		}
		List<T> list = new List<T>();
		foreach (Task<T> item2 in tasks)
		{
			if (item2.Status == TaskStatus.Faulted && item2.Exception != null)
			{
				foreach (Exception innerException in item2.Exception.InnerExceptions)
				{
					PFLog.Default.Error(innerException.Message);
				}
			}
			else
			{
				list.Add(item2.Result);
			}
		}
		callback?.Invoke(list);
	}
}
