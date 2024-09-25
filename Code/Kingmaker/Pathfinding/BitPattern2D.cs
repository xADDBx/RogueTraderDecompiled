using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class BitPattern2D : IEnumerable<Vector2Int>, IEnumerable
{
	private readonly int Size;

	private readonly int HalfSize;

	private BitArray m_Array;

	private List<Vector2Int> m_List = new List<Vector2Int>(1000);

	private IntRect m_Bounds;

	private static FieldInfo m_ArrayField;

	public IntRect Bounds => m_Bounds;

	public int Count { get; private set; }

	static BitPattern2D()
	{
		m_ArrayField = typeof(BitArray).GetField("m_array", BindingFlags.Instance | BindingFlags.NonPublic);
	}

	public BitPattern2D(int size = 1000)
	{
		Size = size;
		HalfSize = Size / 2;
		m_Array = new BitArray(Size * Size);
	}

	public void Clear()
	{
		Array array = (Array)m_ArrayField.GetValue(m_Array);
		Array.Clear(array, 0, array.Length);
		m_List.Clear();
		m_Bounds = default(IntRect);
		Count = 0;
	}

	public void Add(Vector2Int node)
	{
		int num = node.x + HalfSize;
		int num2 = node.y + HalfSize;
		m_Bounds = m_Bounds.ExpandToContain(node.x, node.y);
		if (num < 0 || num >= Size || num2 < 0 || num2 >= Size)
		{
			throw new ArgumentOutOfRangeException($"({node.x}, {node.y}) is out of bounds for BitPattern2D with Size={Size}");
		}
		if (!m_Array[num2 * Size + num])
		{
			m_Array[num2 * Size + num] = true;
			Count++;
			m_List.Add(node);
		}
	}

	public bool Get(Vector2Int node)
	{
		int num = node.x + HalfSize;
		int num2 = node.y + HalfSize;
		if (num < 0 || num >= Size || num2 < 0 || num2 >= Size)
		{
			throw new ArgumentOutOfRangeException($"({node.x}, {node.y}) is out of bounds for BitPattern2D with Size={Size}");
		}
		return m_Array[num2 * Size + num];
	}

	public bool Has(Vector2Int node)
	{
		int num = node.x + HalfSize;
		int num2 = node.y + HalfSize;
		if (num < 0 || num >= Size || num2 < 0 || num2 >= Size)
		{
			return false;
		}
		return m_Array[num2 * Size + num];
	}

	public BitArray GetCulledArray(in BitArray result)
	{
		result.Length = m_Bounds.Width * m_Bounds.Height;
		for (int i = m_Bounds.ymin; i <= m_Bounds.ymax; i++)
		{
			for (int j = m_Bounds.xmin; j <= m_Bounds.xmax; j++)
			{
				result[(i - m_Bounds.ymin) * m_Bounds.Width + (j - m_Bounds.xmin)] = m_Array[(i + HalfSize) * Size + j + HalfSize];
			}
		}
		return result;
	}

	public IEnumerator<Vector2Int> GetEnumerator()
	{
		return ((IEnumerable<Vector2Int>)m_List).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)m_List).GetEnumerator();
	}
}
