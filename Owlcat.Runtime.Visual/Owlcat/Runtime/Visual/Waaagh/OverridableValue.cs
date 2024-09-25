using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Owlcat.Runtime.Visual.Waaagh;

public struct OverridableValue<T>
{
	public interface ISource
	{
		T Value { get; }
	}

	private readonly struct OverrideSourceData
	{
		public readonly ISource source;

		public readonly int priority;

		public OverrideSourceData(ISource source, int priority)
		{
			this.source = source;
			this.priority = priority;
		}
	}

	private readonly Func<T> m_DefaultValueGetter;

	private List<OverrideSourceData> m_OverrideSourceDataList;

	public OverridableValue(Func<T> defaultValueGetter)
	{
		m_DefaultValueGetter = defaultValueGetter;
		m_OverrideSourceDataList = null;
	}

	public static implicit operator T(in OverridableValue<T> value)
	{
		return value.Resolve();
	}

	[Pure]
	public T Resolve()
	{
		if (m_OverrideSourceDataList != null && m_OverrideSourceDataList.Count > 0)
		{
			return m_OverrideSourceDataList[0].source.Value;
		}
		return m_DefaultValueGetter();
	}

	public void AddOverride(ISource source, int priority)
	{
		if (m_OverrideSourceDataList != null)
		{
			int i = 0;
			for (int count = m_OverrideSourceDataList.Count; i < count; i++)
			{
				if (m_OverrideSourceDataList[i].priority <= priority)
				{
					m_OverrideSourceDataList.Insert(i, new OverrideSourceData(source, priority));
					return;
				}
			}
		}
		else
		{
			m_OverrideSourceDataList = new List<OverrideSourceData>();
		}
		m_OverrideSourceDataList.Add(new OverrideSourceData(source, priority));
	}

	public void RemoveOverride(ISource source)
	{
		int i = 0;
		for (int count = m_OverrideSourceDataList.Count; i < count; i++)
		{
			if (m_OverrideSourceDataList[i].source == source)
			{
				m_OverrideSourceDataList.RemoveAt(i);
				break;
			}
		}
	}
}
