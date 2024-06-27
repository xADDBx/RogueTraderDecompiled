using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Utility.DotNetExtensions;

public class TempHashSet
{
	private abstract class Releasable
	{
		public abstract void ReleaseInternal();
	}

	private class PoolHolder<T> : Releasable
	{
		public static readonly PoolHolder<T> Instance;

		public readonly Stack<HashSet<T>> Pool = new Stack<HashSet<T>>();

		public readonly Stack<HashSet<T>> Claimed = new Stack<HashSet<T>>();

		static PoolHolder()
		{
			Pools.Add(Instance = new PoolHolder<T>());
		}

		public override void ReleaseInternal()
		{
			while (Claimed.Count > 0)
			{
				HashSet<T> hashSet = Claimed.Pop();
				hashSet.Clear();
				Pool.Push(hashSet);
			}
		}
	}

	private static readonly List<Releasable> Pools = new List<Releasable>();

	public static HashSet<T> Get<T>()
	{
		if (!UnityThreadHolder.IsMainThread)
		{
			throw new InvalidOperationException("This should not be used from other threads");
		}
		HashSet<T> hashSet = ((PoolHolder<T>.Instance.Pool.Count > 0) ? PoolHolder<T>.Instance.Pool.Pop() : new HashSet<T>());
		PoolHolder<T>.Instance.Claimed.Push(hashSet);
		return hashSet;
	}

	public static void Release()
	{
		foreach (Releasable pool in Pools)
		{
			pool.ReleaseInternal();
		}
	}
}
