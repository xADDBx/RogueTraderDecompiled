using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Unity.IL2CPP.CompilerServices;

namespace Kingmaker.EntitySystem;

public struct EntityPoolEnumerator<T> : IEnumerator<T>, IEnumerator, IDisposable where T : Entity
{
	private readonly List<T> m_Source;

	private int m_CurrentIndex;

	private T m_Current;

	public T Current => m_Current;

	object IEnumerator.Current => m_Current;

	public EntityPoolEnumerator(List<T> source)
	{
		this = default(EntityPoolEnumerator<T>);
		m_Source = source;
		m_CurrentIndex = -1;
	}

	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOption(Option.NullChecks, false)]
	public bool MoveNext()
	{
		while (++m_CurrentIndex < m_Source.Count)
		{
			T val = m_Source[m_CurrentIndex];
			if (val.ShouldBeEnumeratedByEntityPoolEnumerator)
			{
				m_Current = val;
				return true;
			}
		}
		m_Current = null;
		return false;
	}

	public void Reset()
	{
		throw new NotImplementedException("Reset is not valid");
	}

	public void Dispose()
	{
	}
}
