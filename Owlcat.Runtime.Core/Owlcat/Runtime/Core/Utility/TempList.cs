using System.Collections.Generic;

namespace Owlcat.Runtime.Core.Utility;

public class TempList
{
	private abstract class Releasable
	{
		public abstract void ReleaseInternal();
	}

	private class PoolHolder<T> : Releasable
	{
		public static readonly PoolHolder<T> Instance;

		public readonly Stack<List<T>> Pool = new Stack<List<T>>();

		public readonly Stack<List<T>> Claimed = new Stack<List<T>>();

		static PoolHolder()
		{
			s_Pools.Add(Instance = new PoolHolder<T>());
		}

		public override void ReleaseInternal()
		{
			while (Claimed.Count > 0)
			{
				List<T> list = Claimed.Pop();
				list.Clear();
				Pool.Push(list);
			}
		}
	}

	private static readonly List<Releasable> s_Pools = new List<Releasable>();

	public static List<T> Get<T>()
	{
		List<T> list = ((PoolHolder<T>.Instance.Pool.Count > 0) ? PoolHolder<T>.Instance.Pool.Pop() : new List<T>());
		PoolHolder<T>.Instance.Claimed.Push(list);
		return list;
	}

	public static void Release()
	{
		foreach (Releasable s_Pool in s_Pools)
		{
			s_Pool.ReleaseInternal();
		}
	}
}
