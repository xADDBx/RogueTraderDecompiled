using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct ShadowStatisticsWriter
{
	private NativeReference<ShadowStatistics> m_StatisticsReference;

	public ShadowStatisticsWriter(NativeReference<ShadowStatistics> statisticsReference)
	{
		m_StatisticsReference = statisticsReference;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetCachedCounter(int value)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetUseCandidateCounter(int value)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetUseCounter(int value)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void IncrementRenderCandidateCounter()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void IncrementRenderCounter()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void IncrementUpdateRenderDataCandidateCounter()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void IncrementUpdateRenderDataCounter()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetDynamicAtlasOverflowDetected()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetCachedAtlasOverflowDetected()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetConstantBufferOverflowDetected()
	{
	}
}
