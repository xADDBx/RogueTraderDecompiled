using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public readonly struct PatternGridData : IDisposable, IReadOnlyCollection<Vector2Int>, IEnumerable<Vector2Int>, IEnumerable
{
	public struct Enumerator : IEnumerator<Vector2Int>, IEnumerator, IDisposable
	{
		private readonly BitArray m_Offsets;

		private readonly IntRect m_Bounds;

		private int m_Current;

		public Vector2Int Current
		{
			get
			{
				if (m_Current < 0 || m_Current >= m_Offsets.Length)
				{
					throw new InvalidOperationException("Iterator is in invalid state");
				}
				return new Vector2Int(m_Bounds.xmin + m_Current % m_Bounds.Width, m_Bounds.ymin + m_Current / m_Bounds.Width);
			}
		}

		object IEnumerator.Current => Current;

		public Enumerator(BitArray offsets, IntRect bounds)
		{
			this = default(Enumerator);
			m_Offsets = offsets;
			m_Bounds = bounds;
			Reset();
		}

		public bool MoveNext()
		{
			do
			{
				m_Current++;
				if (m_Current >= m_Offsets.Length)
				{
					return false;
				}
			}
			while (!m_Offsets[m_Current]);
			return true;
		}

		public void Reset()
		{
			m_Current = -1;
		}

		public void Dispose()
		{
			Reset();
		}
	}

	public class ObjectPool<T>
	{
		private readonly ConcurrentBag<T> m_Objects;

		private readonly Func<T> m_ObjectGenerator;

		private readonly Action<T> m_Prepare;

		private readonly Action<T> m_Cleanup;

		public ObjectPool(Func<T> objectGenerator, Action<T> prepare, Action<T> cleanup)
		{
			m_ObjectGenerator = objectGenerator ?? throw new ArgumentNullException("objectGenerator");
			m_Prepare = prepare;
			m_Cleanup = cleanup;
			m_Objects = new ConcurrentBag<T>();
		}

		public T Get()
		{
			if (!m_Objects.TryTake(out var result))
			{
				result = m_ObjectGenerator();
			}
			m_Prepare(result);
			return result;
		}

		public void Return(T item)
		{
			m_Cleanup(item);
			m_Objects.Add(item);
		}
	}

	public static PatternGridData Empty = new PatternGridData(new BitArray(0), new IntRect(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue), 0, disposable: false);

	private static readonly IntRect Invalid = new IntRect(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);

	private static readonly ObjectPool<BitArray> Pool = new ObjectPool<BitArray>(() => new BitArray(0), delegate
	{
	}, delegate
	{
	});

	private readonly BitArray m_Data;

	private readonly IntRect m_Bounds;

	private readonly bool m_Disposable;

	private readonly int m_Count;

	public IntRect Bounds => m_Bounds;

	public bool IsEmpty => m_Data.Length == 0;

	public int Count => m_Count;

	public bool Contains(Vector2Int item)
	{
		bool num = item.x >= m_Bounds.xmin && item.x <= m_Bounds.xmax;
		bool flag = item.y >= m_Bounds.ymin && item.y <= m_Bounds.ymax;
		if (!num || !flag)
		{
			return false;
		}
		int num2 = (item.y - m_Bounds.ymin) * m_Bounds.Width + (item.x - m_Bounds.xmin);
		if (num2 < 0 || num2 >= m_Data.Length)
		{
			PFLog.Default.Error($"Coordinates are out of pattern: {item}");
			return false;
		}
		return m_Data[num2];
	}

	public PatternGridData Move(Vector2Int offset)
	{
		return new PatternGridData(m_Data, m_Bounds.Move(offset), m_Count, m_Disposable);
	}

	public static PatternGridData Create(HashSet<Vector2Int> pattern, bool disposable)
	{
		if (pattern.Count == 0)
		{
			return Empty;
		}
		BitArray bitArray = Pool.Get();
		IntRect bounds = ToBitArray(in pattern, bitArray);
		return new PatternGridData(bitArray, bounds, pattern.Count, disposable);
	}

	public static PatternGridData Create(Linecast.Ray2NodeOffsets pattern, int length, bool disposable)
	{
		if (length == 0)
		{
			return Empty;
		}
		BitArray bitArray = Pool.Get();
		IntRect bounds = ToBitArray(in pattern, length, bitArray);
		return new PatternGridData(bitArray, bounds, length, disposable);
	}

	private static IntRect ToBitArray(in HashSet<Vector2Int> nodes, BitArray result)
	{
		if (nodes.Count == 0)
		{
			result.Length = 0;
			return Invalid;
		}
		IntRect result2 = Invalid;
		foreach (Vector2Int node in nodes)
		{
			result2 = result2.ExpandToContain(node.x, node.y);
		}
		Vector2Int vector2Int = new Vector2Int(result2.xmin, result2.ymin);
		result.Length = result2.Width * result2.Height;
		result.SetAll(value: false);
		foreach (Vector2Int node2 in nodes)
		{
			Vector2Int vector2Int2 = node2 - vector2Int;
			int index = vector2Int2.y * result2.Width + vector2Int2.x;
			result[index] = true;
		}
		return result2;
	}

	private static IntRect ToBitArray(in Linecast.Ray2NodeOffsets nodes, int length, BitArray result)
	{
		if (length == 0)
		{
			result.Length = 0;
			return Invalid;
		}
		IntRect result2 = Invalid;
		int num = 0;
		foreach (Vector2Int node in nodes)
		{
			if (++num > length)
			{
				break;
			}
			result2 = result2.ExpandToContain(node.x, node.y);
		}
		Vector2Int vector2Int = new Vector2Int(result2.xmin, result2.ymin);
		result.Length = result2.Width * result2.Height;
		result.SetAll(value: false);
		num = 0;
		foreach (Vector2Int node2 in nodes)
		{
			if (++num > length)
			{
				break;
			}
			Vector2Int vector2Int2 = node2 - vector2Int;
			int index = vector2Int2.y * result2.Width + vector2Int2.x;
			result[index] = true;
		}
		return result2;
	}

	private PatternGridData(BitArray data, IntRect bounds, int count, bool disposable)
	{
		m_Data = data;
		m_Bounds = bounds;
		m_Bounds = bounds;
		m_Disposable = disposable;
		m_Count = count;
	}

	public void Dispose()
	{
		if (m_Disposable)
		{
			Pool.Return(m_Data);
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(m_Data, m_Bounds);
	}

	IEnumerator<Vector2Int> IEnumerable<Vector2Int>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
