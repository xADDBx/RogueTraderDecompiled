using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Owlcat.QA.Validation;

public class ValidationStack<T> : IDisposable where T : class, IValidated
{
	private class IdentityEqualityComparer : IEqualityComparer<T>
	{
		public bool Equals(T left, T right)
		{
			return left == right;
		}

		public int GetHashCode(T value)
		{
			return RuntimeHelpers.GetHashCode(value);
		}
	}

	private readonly LinkedList<T> m_ValidationStack = new LinkedList<T>();

	private readonly Dictionary<T, int> m_ElementsCounter = new Dictionary<T, int>(new IdentityEqualityComparer());

	public bool HasCircularDependencies => m_ValidationStack.Count > m_ElementsCounter.Count;

	public TV FindElementOfType<TV>() where TV : class, T
	{
		foreach (T item in m_ValidationStack.Reverse())
		{
			if (item is TV result)
			{
				return result;
			}
		}
		return null;
	}

	public string FormatValidationStack()
	{
		if (m_ValidationStack.Count <= 1)
		{
			return "";
		}
		return "Validation chain: " + string.Join("->", m_ValidationStack.Select((T v) => v.ToString()));
	}

	public void Clear()
	{
		m_ValidationStack.Clear();
	}

	public void Push([NotNull] T obj)
	{
		m_ValidationStack.AddLast(obj);
		m_ElementsCounter.TryGetValue(obj, out var value);
		m_ElementsCounter[obj] = value + 1;
	}

	public void Dispose()
	{
		Pop();
	}

	private void Pop()
	{
		T value = m_ValidationStack.Last.Value;
		m_ValidationStack.RemoveLast();
		m_ElementsCounter.TryGetValue(value, out var value2);
		if (value2 == 1)
		{
			m_ElementsCounter.Remove(value);
		}
		else
		{
			m_ElementsCounter[value] = value2 - 1;
		}
	}
}
