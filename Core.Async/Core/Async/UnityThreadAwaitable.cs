using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Core.Async;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct UnityThreadAwaitable : INotifyCompletion
{
	private static readonly SendOrPostCallback _callback = delegate(object obj)
	{
		((Action)obj)();
	};

	public bool IsCompleted => UnitySyncContextHolder.UnitySynchronizationContext == SynchronizationContext.Current;

	public UnityThreadAwaitable GetAwaiter()
	{
		return default(UnityThreadAwaitable);
	}

	public void OnCompleted(Action continuation)
	{
		UnitySyncContextHolder.UnitySynchronizationContext.Post(_callback, continuation);
	}

	public void GetResult()
	{
	}
}
