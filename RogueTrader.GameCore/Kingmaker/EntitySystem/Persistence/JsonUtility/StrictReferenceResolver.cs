using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility;

public class StrictReferenceResolver : IReferenceResolver
{
	private class References
	{
		private class ReferenceComparer : IEqualityComparer<object>
		{
			bool IEqualityComparer<object>.Equals(object x, object y)
			{
				return x == y;
			}

			int IEqualityComparer<object>.GetHashCode(object obj)
			{
				return RuntimeHelpers.GetHashCode(obj);
			}
		}

		public static readonly IEqualityComparer<object> Comparer = new ReferenceComparer();

		private int m_ReferenceCount;

		[NotNull]
		private readonly Dictionary<string, object> m_ObjectByReference = new Dictionary<string, object>();

		[NotNull]
		private readonly Dictionary<object, string> m_ReferenceByObject = new Dictionary<object, string>(Comparer);

		public object ResolveReference(string reference)
		{
			if (!m_ObjectByReference.ContainsKey(reference))
			{
				throw new JsonSerializationException("Object doesnt exist for reference " + reference);
			}
			return m_ObjectByReference[reference];
		}

		public string GetReference(object value)
		{
			if (!m_ReferenceByObject.ContainsKey(value))
			{
				m_ReferenceCount++;
				string value2 = m_ReferenceCount.ToString(CultureInfo.InvariantCulture);
				m_ReferenceByObject[value] = value2;
			}
			return m_ReferenceByObject[value];
		}

		public bool IsReferenced(object value)
		{
			return m_ReferenceByObject.ContainsKey(value);
		}

		public void AddReference(string reference, object value)
		{
			if (m_ObjectByReference.ContainsKey(reference))
			{
				throw new JsonSerializationException($"Duplicate reference key: {reference} (old = {m_ObjectByReference[reference]}, new = {value})");
			}
			if (m_ReferenceByObject.ContainsKey(value))
			{
				throw new JsonSerializationException($"Duplicate reference to object {value}: reference (old = {m_ReferenceByObject[value]}, new = {reference})");
			}
			m_ObjectByReference[reference] = value;
			m_ReferenceByObject[value] = reference;
		}
	}

	[NotNull]
	private readonly Dictionary<object, References> m_Contexts = new Dictionary<object, References>();

	[NotNull]
	public static StrictReferenceResolver Instance = new StrictReferenceResolver();

	public void ClearContexts()
	{
		m_Contexts.Clear();
	}

	[NotNull]
	private References GetReferences(object context)
	{
		if (!m_Contexts.TryGetValue(context, out var value))
		{
			value = new References();
			m_Contexts[context] = value;
		}
		return value;
	}

	public object ResolveReference(object context, string reference)
	{
		return GetReferences(context).ResolveReference(reference);
	}

	public string GetReference(object context, object value)
	{
		return GetReferences(context).GetReference(value);
	}

	public bool IsReferenced(object context, object value)
	{
		return GetReferences(context).IsReferenced(value);
	}

	public void AddReference(object context, string reference, object value)
	{
		GetReferences(context).AddReference(reference, value);
	}
}
