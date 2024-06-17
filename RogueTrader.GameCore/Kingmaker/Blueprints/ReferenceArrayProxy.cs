using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Blueprints;

public readonly struct ReferenceArrayProxy<T> : IEnumerable<T>, IEnumerable where T : BlueprintScriptableObject
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		[CanBeNull]
		private readonly BlueprintReference<T>[] m_Array;

		private int m_Index;

		public T Current
		{
			get
			{
				BlueprintReference<T>[] array = m_Array;
				if (array == null)
				{
					return null;
				}
				BlueprintReference<T> blueprintReference = array.Get(m_Index);
				if (blueprintReference == null)
				{
					return null;
				}
				return blueprintReference.Get();
			}
		}

		object IEnumerator.Current => Current;

		public Enumerator([CanBeNull] BlueprintReference<T>[] array)
		{
			m_Array = array;
			m_Index = -1;
		}

		public bool MoveNext()
		{
			if (m_Array != null)
			{
				return ++m_Index < m_Array.Length;
			}
			return false;
		}

		public void Reset()
		{
			m_Index = -1;
		}

		public void Dispose()
		{
		}
	}

	[CanBeNull]
	private readonly BlueprintReference<T>[] m_Array;

	public T this[int i]
	{
		get
		{
			BlueprintReference<T>[] array = m_Array;
			if (array == null)
			{
				return null;
			}
			BlueprintReference<T> obj = array[i];
			if (obj == null)
			{
				return null;
			}
			return obj.Get();
		}
	}

	public int Length
	{
		get
		{
			BlueprintReference<T>[] array = m_Array;
			if (array == null)
			{
				return 0;
			}
			return array.Length;
		}
	}

	public ReferenceArrayProxy([CanBeNull] BlueprintReference<T>[] array)
	{
		m_Array = array;
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		if (m_Array != null)
		{
			BlueprintReference<T>[] array = m_Array;
			foreach (BlueprintReference<T> blueprintReference in array)
			{
				yield return blueprintReference.Get();
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (m_Array != null)
		{
			BlueprintReference<T>[] array = m_Array;
			foreach (BlueprintReference<T> blueprintReference in array)
			{
				yield return blueprintReference.Get();
			}
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(m_Array);
	}

	public bool HasReference(BlueprintScriptableObject bp)
	{
		return m_Array.Any((BlueprintReference<T> r) => r.Is(bp));
	}

	public bool HasReference(BlueprintReference<T> r1)
	{
		return m_Array.Any((BlueprintReference<T> r) => r.Guid == r1.Guid);
	}

	public int IndexOf(T bp)
	{
		if (m_Array == null)
		{
			return -1;
		}
		for (int i = 0; i < m_Array.Length; i++)
		{
			if (m_Array[i].Is(bp))
			{
				return i;
			}
		}
		return -1;
	}

	public static implicit operator BlueprintReference<T>[](ReferenceArrayProxy<T> proxy)
	{
		return proxy.m_Array;
	}

	public static implicit operator ReferenceArrayProxy<T>(BlueprintReference<T>[] array)
	{
		return new ReferenceArrayProxy<T>(array);
	}
}
