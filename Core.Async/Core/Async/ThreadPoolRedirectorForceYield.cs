using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Core.Async;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct ThreadPoolRedirectorForceYield : ICriticalNotifyCompletion, INotifyCompletion
{
	private static readonly WaitCallback _callback = delegate(object obj)
	{
		((Action)obj)();
	};

	public bool IsCompleted => false;

	public ThreadPoolRedirectorForceYield GetAwaiter()
	{
		return default(ThreadPoolRedirectorForceYield);
	}

	public void OnCompleted(Action continuation)
	{
		ThreadPool.QueueUserWorkItem(_callback, continuation);
	}

	public void UnsafeOnCompleted(Action continuation)
	{
		ThreadPool.UnsafeQueueUserWorkItem(_callback, continuation);
	}

	public void GetResult()
	{
	}
}
