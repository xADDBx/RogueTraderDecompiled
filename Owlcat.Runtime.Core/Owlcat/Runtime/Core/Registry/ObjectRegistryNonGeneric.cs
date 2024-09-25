using System;
using System.Collections;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Logging;

namespace Owlcat.Runtime.Core.Registry;

public class ObjectRegistryNonGeneric : IObjectRegistryBase, IEnumerable
{
	private struct EnumeratorWrapper : IEnumerator, IDisposable
	{
		private readonly ObjectRegistryNonGeneric m_Owner;

		private readonly IEnumerator<object> m_EnumeratorImplementation;

		public object Current => m_EnumeratorImplementation.Current;

		public EnumeratorWrapper(ObjectRegistryNonGeneric owner)
		{
			m_Owner = owner;
			m_EnumeratorImplementation = m_Owner.m_Registry.GetEnumerator();
		}

		public bool MoveNext()
		{
			return m_EnumeratorImplementation.MoveNext();
		}

		public void Reset()
		{
			m_EnumeratorImplementation.Reset();
		}

		public void Dispose()
		{
			m_Owner.m_EnumeratorGuard--;
			m_EnumeratorImplementation.Reset();
		}
	}

	private readonly HashSet<object> m_Registry = new HashSet<object>();

	private int m_EnumeratorGuard;

	public uint Version { get; private set; }

	internal int EnumeratorGuard
	{
		get
		{
			return m_EnumeratorGuard;
		}
		set
		{
			m_EnumeratorGuard = value;
		}
	}

	public IEnumerator GetEnumerator()
	{
		m_EnumeratorGuard++;
		return new EnumeratorWrapper(this);
	}

	public ObjectRegistryEnumerator<T2> GetEnumerator<T2>()
	{
		return new ObjectRegistryEnumerator<T2>(this);
	}

	void IObjectRegistryBase.Register(object obj)
	{
		Register(obj);
	}

	void IObjectRegistryBase.Delete(object obj)
	{
		Delete(obj);
	}

	internal bool Register(object obj)
	{
		if (m_Registry.Add(obj))
		{
			Version++;
			if (m_EnumeratorGuard > 0)
			{
				m_EnumeratorGuard = 0;
				LogChannel.Default.Error($"Registered object {obj} in registry during an enumeration");
			}
			return true;
		}
		return false;
	}

	internal bool Delete(object obj)
	{
		if (m_Registry.Remove(obj))
		{
			Version++;
			if (m_EnumeratorGuard > 0)
			{
				m_EnumeratorGuard = 0;
				LogChannel.Default.Error($"Removed object {obj} from registry during an enumeration");
			}
			return true;
		}
		return false;
	}

	void IObjectRegistryBase.TestClearStaticInstance()
	{
	}

	internal HashSet<object> GetRegistry()
	{
		return m_Registry;
	}
}
