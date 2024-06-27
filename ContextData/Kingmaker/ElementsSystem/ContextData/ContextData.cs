using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.ElementsSystem.ContextData;

public abstract class ContextData
{
	protected class RefIndex
	{
		public int Value;

		public Type Type;
	}

	protected static readonly LogChannel LogChannel = LogChannelFactory.GetOrCreate("ContextData");

	private static readonly List<RefIndex> Indexes = new List<RefIndex>();

	protected static void AddPool(RefIndex index)
	{
		Indexes.Add(index);
	}

	public static void Check()
	{
		foreach (RefIndex index in Indexes)
		{
			if (-1 < index.Value)
			{
				LogChannel.Error($"[ContextData] Unexpected index={index.Value} for type={index.Type}");
			}
		}
	}
}
public abstract class ContextData<TData> : ContextData, IDisposable where TData : ContextData<TData>, new()
{
	private const int MaxPoolSize = 200;

	private static readonly List<TData> Pool;

	private static readonly RefIndex CurrentIndexRef;

	private static int s_CurrentIndex
	{
		get
		{
			return CurrentIndexRef.Value;
		}
		set
		{
			CurrentIndexRef.Value = value;
		}
	}

	[CanBeNull]
	public static TData Current
	{
		get
		{
			if (0 > s_CurrentIndex || s_CurrentIndex >= Pool.Count)
			{
				return null;
			}
			return Pool[s_CurrentIndex];
		}
	}

	public static TData Top
	{
		get
		{
			if (Pool.Count <= 0)
			{
				return null;
			}
			return Pool[0];
		}
	}

	public static int CurrentDepth => s_CurrentIndex;

	static ContextData()
	{
		Pool = new List<TData>();
		CurrentIndexRef = new RefIndex
		{
			Value = -1,
			Type = typeof(TData)
		};
		ContextData.AddPool(CurrentIndexRef);
	}

	protected abstract void Reset();

	[NotNull]
	public static TData Request()
	{
		if (s_CurrentIndex >= 200)
		{
			throw new Exception($"Sort-of stack overflow exception. Too many ({s_CurrentIndex}) context objects of type {typeof(TData).Name}");
		}
		s_CurrentIndex++;
		if (s_CurrentIndex >= Pool.Count)
		{
			Pool.Add(new TData());
		}
		return Current;
	}

	[CanBeNull]
	public static TData RequestIf(bool condition)
	{
		if (!condition)
		{
			return null;
		}
		return Request();
	}

	public void Dispose()
	{
		if (s_CurrentIndex < 0)
		{
			LogChannel.System.Error(GetType().Name + ".Dispose: s_CurrentIndex < 0");
			return;
		}
		Current?.Reset();
		s_CurrentIndex--;
	}

	public static implicit operator bool([CanBeNull] ContextData<TData> data)
	{
		return data != null;
	}
}
