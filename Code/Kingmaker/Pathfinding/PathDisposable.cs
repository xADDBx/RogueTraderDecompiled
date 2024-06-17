using System;
using System.Collections.Generic;
using Pathfinding;

namespace Kingmaker.Pathfinding;

public class PathDisposable<T> : IDisposable where T : Path
{
	private static Stack<PathDisposable<T>> s_Pool = new Stack<PathDisposable<T>>();

	private object m_Owner;

	public T Path { get; private set; }

	public void Init(T path, object owner)
	{
		Path = path;
		m_Owner = owner;
		path?.Claim(m_Owner);
	}

	public void Dispose()
	{
		Path?.Release(m_Owner);
		Path = null;
		m_Owner = null;
		s_Pool.Push(this);
	}

	public static PathDisposable<T> Get(T path, object owner)
	{
		PathDisposable<T> result = null;
		if (!s_Pool.TryPop(out result))
		{
			result = new PathDisposable<T>();
		}
		result.Init(path, owner);
		return result;
	}
}
