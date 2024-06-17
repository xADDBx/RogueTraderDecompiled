using System;
using System.Collections;
using System.Collections.Generic;

namespace Kingmaker.Pathfinding;

public readonly struct NodeList : IEnumerable<CustomGridNodeBase>, IEnumerable, IDisposable
{
	public struct Enumerator : IEnumerator<CustomGridNodeBase>, IEnumerator, IDisposable
	{
		private readonly CustomGridGraph m_Graph;

		private PatternGridData.Enumerator m_PatternEnumerator;

		private CustomGridNodeBase m_Current;

		public CustomGridNodeBase Current => m_Current;

		object IEnumerator.Current => Current;

		public Enumerator(CustomGridGraph graph, PatternGridData.Enumerator enumerator)
		{
			this = default(Enumerator);
			m_Graph = graph;
			m_PatternEnumerator = enumerator;
			Reset();
		}

		public bool MoveNext()
		{
			do
			{
				if (!m_PatternEnumerator.MoveNext())
				{
					return false;
				}
				m_Current = m_Graph.GetNode(m_PatternEnumerator.Current.x, m_PatternEnumerator.Current.y);
			}
			while (m_Current == null);
			return true;
		}

		public void Reset()
		{
			m_PatternEnumerator.Reset();
			m_Current = null;
		}

		public void Dispose()
		{
			m_PatternEnumerator.Dispose();
		}
	}

	public static readonly NodeList Empty = new NodeList(null, in PatternGridData.Empty);

	private readonly CustomGridGraph m_Graph;

	private readonly PatternGridData m_Pattern;

	public bool IsEmpty => m_Pattern.IsEmpty;

	public NodeList(CustomGridGraph graph, in PatternGridData pattern)
	{
		m_Graph = graph;
		m_Pattern = pattern;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(m_Graph, m_Pattern.GetEnumerator());
	}

	IEnumerator<CustomGridNodeBase> IEnumerable<CustomGridNodeBase>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool Contains(CustomGridNodeBase item)
	{
		if (item.Graph != m_Graph)
		{
			return false;
		}
		return m_Pattern.Contains(item.CoordinatesInGrid);
	}

	public void Dispose()
	{
		m_Pattern.Dispose();
	}
}
