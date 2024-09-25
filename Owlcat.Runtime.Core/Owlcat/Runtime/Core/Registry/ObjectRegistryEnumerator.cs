using System;
using System.Collections;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Logging;

namespace Owlcat.Runtime.Core.Registry;

public struct ObjectRegistryEnumerator<T> : IEnumerator<T>, IEnumerator, IDisposable
{
	private readonly ObjectRegistryNonGeneric m_Owner;

	private HashSet<object>.Enumerator m_EnumeratorImplementation;

	public T Current => (T)m_EnumeratorImplementation.Current;

	object IEnumerator.Current => m_EnumeratorImplementation.Current;

	public ObjectRegistryEnumerator(ObjectRegistryNonGeneric owner)
	{
		m_Owner = owner;
		m_EnumeratorImplementation = m_Owner.GetRegistry().GetEnumerator();
	}

	private bool MoveNext(ref HashSet<object>.Enumerator enumerator)
	{
		return enumerator.MoveNext();
	}

	public bool MoveNext()
	{
		return MoveNext(ref m_EnumeratorImplementation);
	}

	public void Reset()
	{
		((IEnumerator)m_EnumeratorImplementation).Reset();
	}

	public void Dispose()
	{
		try
		{
			m_Owner.EnumeratorGuard--;
		}
		catch (Exception ex)
		{
			LogChannel.Default.Exception(ex, "Exception in registry for " + typeof(T).Name);
		}
	}
}
