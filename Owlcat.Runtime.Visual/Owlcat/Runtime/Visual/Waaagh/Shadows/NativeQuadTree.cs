using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct NativeQuadTree
{
	public const short kMinQuadResolution = 128;

	private NativeArray<NativeQuadTreeNode> m_Nodes;

	private NativeStack<short> m_IndicesStack;

	private readonly short m_Levels;

	private NativeArray<short> m_LevelStartIndices;

	private NativeArray<short> m_LevelNodeCount;

	private readonly int m_Resolution;

	public bool IsCreated => m_Nodes.IsCreated;

	public int Resolution => m_Resolution;

	public int Levels => m_Levels;

	public NativeQuadTree(short levels, int resolution, Allocator allocator)
	{
		m_Levels = levels;
		m_Resolution = resolution;
		m_LevelNodeCount = new NativeArray<short>(m_Levels, allocator, NativeArrayOptions.UninitializedMemory);
		m_LevelStartIndices = new NativeArray<short>(m_Levels, allocator, NativeArrayOptions.UninitializedMemory);
		short num = 0;
		for (short num2 = 0; num2 < m_Levels; num2++)
		{
			num += (short)math.pow(4f, num2);
		}
		if (num > short.MaxValue)
		{
			throw new ArgumentException("Nodes count is too big");
		}
		m_IndicesStack = new NativeStack<short>(4, allocator);
		m_Nodes = new NativeArray<NativeQuadTreeNode>(num, allocator);
		m_Nodes[0] = new NativeQuadTreeNode
		{
			State = NativeQuadTreeNodeState.Free,
			Rect = new float4(0f, 0f, resolution, resolution),
			ParentIndex = -1,
			ChildrenStartIndex = -1
		};
		m_LevelStartIndices[0] = 0;
		m_LevelNodeCount[0] = 1;
		short num3 = 1;
		for (short num4 = 1; num4 < m_Levels; num4++)
		{
			short num5 = (short)Mathf.Pow(4f, num4);
			m_LevelNodeCount[num4] = num5;
			m_LevelStartIndices[num4] = (short)(m_LevelStartIndices[num4 - 1] + m_LevelNodeCount[num4 - 1]);
			for (int i = 0; i < num5; i++)
			{
				short num6 = num3;
				short parentIndex = GetParentIndex(num6);
				NativeQuadTreeNode value = m_Nodes[parentIndex];
				int num7 = (int)(value.Rect.z / 2f);
				int num8 = i % 4;
				int num9 = num8 / 2;
				int num10 = num8 % 2;
				NativeQuadTreeNode value2 = new NativeQuadTreeNode
				{
					State = NativeQuadTreeNodeState.Free,
					Rect = new float4(value.Rect.x + (float)(num9 * num7), value.Rect.y + (float)(num10 * num7), num7, num7),
					ParentIndex = parentIndex,
					ChildrenStartIndex = -1
				};
				if (num8 == 0)
				{
					value.ChildrenStartIndex = num6;
				}
				m_Nodes[parentIndex] = value;
				m_Nodes[num6] = value2;
				num3++;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetNodesCountOnLevel(int level)
	{
		return m_LevelNodeCount[level];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetLevelStartIndex(int level)
	{
		return m_LevelStartIndices[level];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeQuadTreeNode GetNode(int nodeIndex)
	{
		return m_Nodes[nodeIndex];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private short GetLevel(short index)
	{
		for (short num = 0; num < m_Levels; num++)
		{
			if (m_LevelStartIndices[num] + m_LevelNodeCount[num] > index)
			{
				return num;
			}
		}
		return -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private short GetParentIndex(short index)
	{
		short level = GetLevel(index);
		short index2 = (short)(level - 1);
		short num = m_LevelStartIndices[index2];
		short num2 = (short)(index - m_LevelStartIndices[level]);
		return (short)(num + num2 / 4);
	}

	public bool Allocate(int entryResolution, out float4 rect, out int slotIndex)
	{
		if (entryResolution > Resolution)
		{
			throw new ArgumentException("Entry resolution is too big.", "entryResolution");
		}
		rect = default(float4);
		short num = (short)(entryResolution / 128);
		num = (short)(Mathf.Log(num, 2f) + 1f);
		num = (short)(m_Levels - num);
		short num2 = m_LevelStartIndices[num];
		short num3 = m_LevelNodeCount[num];
		short num4 = -1;
		for (short num5 = 0; num5 < num3; num5++)
		{
			if (m_Nodes[num5 + num2].State == NativeQuadTreeNodeState.Free)
			{
				num4 = (short)(num5 + num2);
				break;
			}
		}
		if (num4 > -1)
		{
			NativeQuadTreeNode nativeQuadTreeNode = m_Nodes[num4];
			rect = nativeQuadTreeNode.Rect;
			if (nativeQuadTreeNode.ParentIndex > -1)
			{
				m_IndicesStack.Push(nativeQuadTreeNode.ParentIndex);
				while (m_IndicesStack.Count > 0)
				{
					short index = m_IndicesStack.Pop();
					NativeQuadTreeNode value = m_Nodes[index];
					value.State = NativeQuadTreeNodeState.PartiallyOccupied;
					m_Nodes[index] = value;
					if (value.ParentIndex > -1)
					{
						m_IndicesStack.Push(value.ParentIndex);
					}
				}
			}
			if (nativeQuadTreeNode.ChildrenStartIndex > -1)
			{
				m_IndicesStack.Push(num4);
				while (m_IndicesStack.Count > 0)
				{
					short num6 = m_IndicesStack.Pop();
					NativeQuadTreeNode value2 = m_Nodes[num6];
					if (num6 == num4)
					{
						value2.State = NativeQuadTreeNodeState.Occupied;
					}
					else
					{
						value2.State = NativeQuadTreeNodeState.OccupiedInHierarchy;
					}
					m_Nodes[num6] = value2;
					if (value2.ChildrenStartIndex > -1)
					{
						for (short num7 = 0; num7 < 4; num7++)
						{
							m_IndicesStack.Push(value2.GetChildIndex(num7));
						}
					}
				}
			}
			else
			{
				NativeQuadTreeNode value3 = m_Nodes[num4];
				value3.State = NativeQuadTreeNodeState.Occupied;
				m_Nodes[num4] = value3;
			}
		}
		slotIndex = num4;
		return num4 >= 0;
	}

	public void Free(int index)
	{
		if (index > m_Nodes.Length - 1 || index < 0)
		{
			return;
		}
		NativeQuadTreeNode nativeQuadTreeNode = m_Nodes[index];
		if (nativeQuadTreeNode.State != NativeQuadTreeNodeState.Occupied)
		{
			return;
		}
		m_IndicesStack.Push((short)index);
		while (m_IndicesStack.Count > 0)
		{
			short index2 = m_IndicesStack.Pop();
			NativeQuadTreeNode value = m_Nodes[index2];
			value.State = NativeQuadTreeNodeState.Free;
			m_Nodes[index2] = value;
			if (value.ChildrenStartIndex > -1)
			{
				for (short num = 0; num < 4; num++)
				{
					m_IndicesStack.Push(value.GetChildIndex(num));
				}
			}
		}
		if (nativeQuadTreeNode.ParentIndex <= -1)
		{
			return;
		}
		m_IndicesStack.Push(nativeQuadTreeNode.ParentIndex);
		while (m_IndicesStack.Count > 0)
		{
			short index3 = m_IndicesStack.Pop();
			NativeQuadTreeNode value2 = m_Nodes[index3];
			int num2 = 0;
			for (short num3 = 0; num3 < 4; num3++)
			{
				if (m_Nodes[value2.GetChildIndex(num3)].State != 0)
				{
					num2++;
				}
			}
			if (num2 == 0)
			{
				value2.State = NativeQuadTreeNodeState.Free;
				m_Nodes[index3] = value2;
			}
			if (value2.ParentIndex > -1)
			{
				m_IndicesStack.Push(value2.ParentIndex);
			}
		}
	}

	public void Dispose()
	{
		if (m_Nodes.IsCreated)
		{
			m_Nodes.Dispose();
		}
		if (m_LevelNodeCount.IsCreated)
		{
			m_LevelNodeCount.Dispose();
		}
		if (m_LevelStartIndices.IsCreated)
		{
			m_LevelStartIndices.Dispose();
		}
		if (m_IndicesStack.IsCreated)
		{
			m_IndicesStack.Dispose();
		}
	}
}
