using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine.Scripting;

namespace Owlcat.Runtime.Core.Registry;

[Preserve]
public class ObjectRegistry<T> : IObjectRegistryBase, IEnumerable<T>, IEnumerable where T : class
{
	private static ObjectRegistry<T> s_Instance;

	private readonly ObjectRegistryNonGeneric m_NonGeneric;

	private ServiceProxy<Repository> m_RepoProxy;

	public static ObjectRegistry<T> Instance
	{
		get
		{
			if (s_Instance?.m_RepoProxy.Instance == null)
			{
				s_Instance = Repository.Instance?.GetRegistry<T>();
				if (s_Instance != null)
				{
					s_Instance.m_RepoProxy = Services.GetProxy<Repository>();
				}
			}
			return s_Instance;
		}
	}

	public T Single
	{
		get
		{
			if (MaybeSome == null)
			{
				throw new InvalidOperationException("No Single object of type " + typeof(T).Name);
			}
			if (m_NonGeneric.GetRegistry().Count > 1)
			{
				throw new InvalidOperationException("Registry for " + typeof(T).Name + " contains multiple objects");
			}
			return MaybeSome;
		}
	}

	public T MaybeSingle
	{
		get
		{
			if (m_NonGeneric.GetRegistry().Count > 1)
			{
				throw new InvalidOperationException("Registry for " + typeof(T).Name + " contains multiple objects");
			}
			return MaybeSome;
		}
	}

	public T MaybeSome { get; private set; }

	public uint Version => m_NonGeneric.Version;

	internal ObjectRegistry()
	{
		m_NonGeneric = new ObjectRegistryNonGeneric();
	}

	private ObjectRegistry(ObjectRegistryNonGeneric or)
	{
		m_NonGeneric = or;
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		m_NonGeneric.EnumeratorGuard++;
		return new ObjectRegistryEnumerator<T>(m_NonGeneric);
	}

	public ObjectRegistryEnumerator<T> GetEnumerator()
	{
		m_NonGeneric.EnumeratorGuard++;
		return new ObjectRegistryEnumerator<T>(m_NonGeneric);
	}

	public ObjectRegistryEnumerator<T2> GetEnumerator<T2>()
	{
		m_NonGeneric.EnumeratorGuard++;
		return new ObjectRegistryEnumerator<T2>(m_NonGeneric);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new NotImplementedException("Use GetEnumerator<T2>() to avoid allocations");
	}

	void IObjectRegistryBase.Register(object obj)
	{
		if (m_NonGeneric.Register(obj))
		{
			MaybeSome = (T)obj;
		}
	}

	void IObjectRegistryBase.Delete(object obj)
	{
		if (m_NonGeneric.Delete(obj) && MaybeSome == obj)
		{
			MaybeSome = m_NonGeneric.GetRegistry().FirstOrDefault() as T;
		}
	}

	void IObjectRegistryBase.TestClearStaticInstance()
	{
		s_Instance = null;
	}

	internal static ObjectRegistry<T> CreateFromNonGeneric(ObjectRegistryNonGeneric or)
	{
		return new ObjectRegistry<T>(or)
		{
			MaybeSome = (or.GetRegistry().FirstOrDefault() as T)
		};
	}
}
