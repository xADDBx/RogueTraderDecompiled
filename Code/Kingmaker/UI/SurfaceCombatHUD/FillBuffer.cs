using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct FillBuffer
{
	private const byte kInvalidMaterialId = 0;

	private NativeArray<byte> m_SurfaceFragmentArray;

	private NativeArray<byte> m_SurfaceChunks;

	public int ChunksCount
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Pure]
		get
		{
			return m_SurfaceChunks.Length;
		}
	}

	public byte this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Pure]
		get
		{
			return m_SurfaceFragmentArray[index];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_SurfaceFragmentArray[index] = value;
			int index2 = index / 64;
			m_SurfaceChunks[index2] = 1;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FillBuffer(NativeArray<byte> surfaceFragmentArray, NativeArray<byte> surfaceChunks)
	{
		m_SurfaceFragmentArray = surfaceFragmentArray;
		m_SurfaceChunks = surfaceChunks;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public bool ChunkHasValue(int chunkIndex)
	{
		return m_SurfaceChunks[chunkIndex] != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public bool ChunkHasNoValue(int chunkIndex)
	{
		return m_SurfaceChunks[chunkIndex] == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public bool HasValue(int index)
	{
		return m_SurfaceFragmentArray[index] != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public bool HasNoValue(int index)
	{
		return m_SurfaceFragmentArray[index] == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Clear()
	{
		UnsafeUtility.MemClear(m_SurfaceFragmentArray.GetUnsafePtr(), m_SurfaceFragmentArray.Length);
		UnsafeUtility.MemClear(m_SurfaceChunks.GetUnsafePtr(), m_SurfaceChunks.Length);
	}
}
