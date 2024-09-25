using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Lighting;

[BurstCompile]
public struct AtlasLinearAllocator : IDisposable
{
	private struct Node
	{
		public ushort rightChildId;

		public ushort bottomChildId;

		public int4 rect;
	}

	private const ushort kInvalidId = ushort.MaxValue;

	private readonly int2 m_Size;

	private readonly float4 m_PlacementRectScale;

	private NativeArray<Node> m_Nodes;

	private NativeReference<ushort> m_NextFreeNodeId;

	public int2 Size
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Size;
		}
	}

	public int AllocationCount
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (m_NextFreeNodeId.Value - 1) / 2;
		}
	}

	public AtlasLinearAllocator(int2 size, int capacity, Allocator allocator = Allocator.Persistent)
	{
		m_Size = size;
		m_PlacementRectScale = new float4(1f / (float)size.x, 1f / (float)size.y, 1f / (float)size.x, 1f / (float)size.y);
		m_Nodes = new NativeArray<Node>(1 + capacity * 2, allocator);
		m_NextFreeNodeId = new NativeReference<ushort>(1, allocator);
		Reset();
	}

	public void Dispose()
	{
		m_Nodes.Dispose();
		m_NextFreeNodeId.Dispose();
	}

	public void Reset()
	{
		m_Nodes[0] = new Node
		{
			rightChildId = ushort.MaxValue,
			bottomChildId = ushort.MaxValue,
			rect = new int4(m_Size, 0, 0)
		};
		m_NextFreeNodeId.Value = 1;
	}

	public unsafe bool Allocate(in int2 allocationSize, ref float4 result)
	{
		if (m_NextFreeNodeId.Value >= m_Nodes.Length)
		{
			return false;
		}
		Node* unsafePtr = (Node*)m_Nodes.GetUnsafePtr();
		return Allocate(unsafePtr, unsafePtr, in allocationSize, ref result);
	}

	private unsafe bool Allocate(Node* nodeBuffer, Node* node, in int2 allocationSize, ref float4 result)
	{
		if (node->rightChildId != ushort.MaxValue)
		{
			if (!Allocate(nodeBuffer, nodeBuffer + (int)node->rightChildId, in allocationSize, ref result))
			{
				return Allocate(nodeBuffer, nodeBuffer + (int)node->bottomChildId, in allocationSize, ref result);
			}
			return true;
		}
		if (allocationSize.x > node->rect.x)
		{
			return false;
		}
		if (allocationSize.y > node->rect.y)
		{
			return false;
		}
		node->rightChildId = m_NextFreeNodeId.Value++;
		node->bottomChildId = m_NextFreeNodeId.Value++;
		Node* ptr = nodeBuffer + (int)node->rightChildId;
		Node* ptr2 = nodeBuffer + (int)node->bottomChildId;
		ptr->rightChildId = ushort.MaxValue;
		ptr->bottomChildId = ushort.MaxValue;
		ptr2->rightChildId = ushort.MaxValue;
		ptr2->bottomChildId = ushort.MaxValue;
		if (allocationSize.x >= allocationSize.y)
		{
			ptr->rect.z = node->rect.z + allocationSize.x;
			ptr->rect.w = node->rect.w;
			ptr->rect.x = node->rect.x - allocationSize.x;
			ptr->rect.y = allocationSize.y;
			ptr2->rect.z = node->rect.z;
			ptr2->rect.w = node->rect.w + allocationSize.y;
			ptr2->rect.x = node->rect.x;
			ptr2->rect.y = node->rect.y - allocationSize.y;
		}
		else
		{
			ptr->rect.z = node->rect.z + allocationSize.x;
			ptr->rect.w = node->rect.w;
			ptr->rect.x = node->rect.x - allocationSize.x;
			ptr->rect.y = node->rect.y;
			ptr2->rect.z = node->rect.z;
			ptr2->rect.w = node->rect.w + allocationSize.y;
			ptr2->rect.x = allocationSize.x;
			ptr2->rect.y = node->rect.y - allocationSize.y;
		}
		node->rect.xy = allocationSize;
		result = node->rect * m_PlacementRectScale;
		return true;
	}
}
