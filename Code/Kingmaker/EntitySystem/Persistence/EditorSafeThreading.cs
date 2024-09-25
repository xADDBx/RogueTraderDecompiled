using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Core.Async;
using Core.Cheats;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence;

public static class EditorSafeThreading
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public readonly struct EditorSafeAwaitable : INotifyCompletion
	{
		public bool IsCompleted
		{
			get
			{
				if (!AsyncSaves)
				{
					return default(UnityThreadAwaitable).IsCompleted;
				}
				return default(ThreadPoolRedirectorForceYield).IsCompleted;
			}
		}

		public EditorSafeAwaitable GetAwaiter()
		{
			return default(EditorSafeAwaitable);
		}

		public void OnCompleted(Action continuation)
		{
			if (AsyncSaves)
			{
				default(ThreadPoolRedirectorForceYield).OnCompleted(continuation);
			}
			else
			{
				default(UnityThreadAwaitable).OnCompleted(continuation);
			}
		}

		public void GetResult()
		{
		}
	}

	[Cheat(Name = "Enable_Editor_Async_Save", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static bool AsyncSaves { get; set; } = !Application.isEditor;


	public static EditorSafeAwaitable Awaitable => default(EditorSafeAwaitable);
}
