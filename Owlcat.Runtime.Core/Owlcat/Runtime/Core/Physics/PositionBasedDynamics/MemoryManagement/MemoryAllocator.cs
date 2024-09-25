using System;
using System.Collections.Generic;
using System.Linq;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;

public class MemoryAllocator
{
	private int m_Size;

	private SortedList<int, int> m_FreeBlocks = new SortedList<int, int>();

	public SortedList<int, int> FreeBlocks => m_FreeBlocks;

	public int Size => m_Size;

	public int Stride { get; set; } = -1;


	public MemoryAllocator(int size)
	{
		Resize(size);
	}

	public void Reset()
	{
		m_FreeBlocks.Clear();
		m_FreeBlocks.Add(0, m_Size);
	}

	public void Resize(int newSize)
	{
		m_Size = newSize;
		Reset();
	}

	public bool TryAlloc(int size, out int offset)
	{
		if (size > m_Size)
		{
			throw new ArgumentOutOfRangeException("size");
		}
		offset = -1;
		foreach (KeyValuePair<int, int> freeBlock in m_FreeBlocks)
		{
			if (freeBlock.Value >= size)
			{
				offset = freeBlock.Key;
				break;
			}
		}
		if (offset > -1)
		{
			int num = m_FreeBlocks[offset];
			m_FreeBlocks.Remove(offset);
			if (num > size)
			{
				m_FreeBlocks.Add(offset + size, num - size);
			}
			return true;
		}
		return false;
	}

	public void Free(int offset, int size)
	{
		if (offset >= m_Size)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (offset + size > m_Size)
		{
			throw new ArgumentOutOfRangeException("size");
		}
		if (m_FreeBlocks.ContainsKey(offset))
		{
			if (m_FreeBlocks[offset] < size)
			{
				m_FreeBlocks[offset] = size;
			}
		}
		else
		{
			m_FreeBlocks.Add(offset, size);
		}
		int num = m_FreeBlocks.IndexOfKey(offset);
		int num2 = num - 1;
		if (num2 < 0)
		{
			num2 = 0;
		}
		for (int i = num2; i < m_FreeBlocks.Count - 1; i++)
		{
			int num3 = m_FreeBlocks.Keys[i];
			int num4 = m_FreeBlocks.Values[i];
			int num5 = m_FreeBlocks.Keys[i + 1];
			int num6 = m_FreeBlocks.Values[i + 1];
			if (num5 <= num3 + num4)
			{
				m_FreeBlocks.RemoveAt(i + 1);
				if (i <= num)
				{
					num--;
				}
				i--;
				if (num5 + num6 > num3 + num4)
				{
					m_FreeBlocks[num3] = num5 + num6 - num3;
				}
			}
			else if (i > num)
			{
				break;
			}
		}
	}

	public override string ToString()
	{
		if (Stride > -1)
		{
			return $"Free blocks count: {m_FreeBlocks.Count}, Free memory: {m_FreeBlocks.Values.Sum() * Stride} / {m_Size * Stride} (bytes)";
		}
		return $"Free blocks count: {m_FreeBlocks.Count}, Free memory: {m_FreeBlocks.Values.Sum()} / {m_Size}";
	}
}
