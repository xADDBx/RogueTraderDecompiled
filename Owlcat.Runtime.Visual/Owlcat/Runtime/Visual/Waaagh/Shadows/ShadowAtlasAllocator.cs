using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct ShadowAtlasAllocator : IDisposable
{
	private NativeQuadTree m_QuadTree;

	public int Levels
	{
		get
		{
			if (!m_QuadTree.IsCreated)
			{
				return 0;
			}
			return m_QuadTree.Levels;
		}
	}

	public int Resolution => m_QuadTree.Resolution;

	public NativeQuadTree QuadTree => m_QuadTree;

	public ShadowAtlasAllocator(ShadowResolution shadowResolution, Allocator allocator)
	{
		short num = (short)((int)shadowResolution / 128);
		num = (short)(Mathf.Log(num, 2f) + 1f);
		m_QuadTree = new NativeQuadTree(num, (int)shadowResolution, allocator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Allocate(int resolution, out float4 rect, out int slotIndex)
	{
		return m_QuadTree.Allocate(resolution, out rect, out slotIndex);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Free(int slotIndex)
	{
		m_QuadTree.Free(slotIndex);
	}

	public void Dispose()
	{
		m_QuadTree.Dispose();
	}
}
