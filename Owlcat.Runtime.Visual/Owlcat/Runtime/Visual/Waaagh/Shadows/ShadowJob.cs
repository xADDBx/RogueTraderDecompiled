using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.Collections;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct ShadowJob : IJob
{
	private const int kMaxAge = 50;

	public int CurrentFrameId;

	public bool StaticShadowCacheEnabled;

	public NativeArray<LightDescriptor> LightDescriptorArray;

	public NativeHashMap<int, ShadowData> ShadowDataCacheMap;

	public NativeList<ShadowProcessData> ProcessDataList;

	public NativeList<ShadowRenderRequest> RenderRequestList;

	public NativeList<ShadowRenderRequest> RenderRequestForCacheList;

	public NativeList<ShadowCacheCopyRequest> CacheCopyRequestList;

	public NativeReference<ShadowConstantBuffer> ConstantBufferReference;

	public NativeReference<ShadowCopyCacheConstantBuffer> CachedShadowsCopyConstantBufferReference;

	public ShadowLightDataFactory LightDataFactory;

	public ShadowAtlasViewportAllocator DynamicAtlasAllocator;

	[NativeDisableContainerSafetyRestriction]
	public ShadowAtlasViewportAllocator StaticCachedAtlasAllocator;

	public ShadowDistanceUpdateQualifier ShadowDistanceUpdateQualifier;

	public ShadowRenderDataFactory RenderDataFactory;

	public ShadowStatisticsWriter StatisticsWriter;

	public bool AlwaysUpdateShadows;

	public void Execute()
	{
		PruneShadowDataCache();
		ExecutePhaseOne();
		ExecutePhaseTwo();
		UpdateShadowDataCache();
		StatisticsWriter.SetCachedCounter(ShadowDataCacheMap.Count);
	}

	private unsafe void PruneShadowDataCache()
	{
		int count = ShadowDataCacheMap.Count;
		int* bufferPtr = stackalloc int[count];
		UnsafeListBuffer<int> unsafeListBuffer = new UnsafeListBuffer<int>(bufferPtr, count);
		foreach (KVPair<int, ShadowData> item2 in ShadowDataCacheMap)
		{
			ref ShadowData value = ref item2.Value;
			int num = CurrentFrameId - value.LastVisibleFrameId;
			if (num < 0 || num > 50)
			{
				DynamicAtlasAllocator.Deallocate(ref value.DynamicAtlasData);
				if (StaticShadowCacheEnabled && value.StaticCacheAtlasData.HasAllocation())
				{
					StaticCachedAtlasAllocator.Deallocate(ref value.StaticCacheAtlasData);
				}
				int value2 = item2.Key;
				unsafeListBuffer.Push(in value2);
			}
		}
		Span<int> span = unsafeListBuffer.AsSpan();
		for (int value2 = 0; value2 < span.Length; value2++)
		{
			int key = span[value2];
			ShadowDataCacheMap.Remove(key);
		}
		Span<LightDescriptor> span2 = LightDescriptorArray.AsSpan();
		for (int value2 = 0; value2 < span2.Length; value2++)
		{
			ref LightDescriptor reference = ref span2[value2];
			if (reference.Shadows == LightShadows.None && ShadowDataCacheMap.TryGetValue(reference.LightID, out var item))
			{
				DynamicAtlasAllocator.Deallocate(ref item.DynamicAtlasData);
				if (StaticShadowCacheEnabled && item.StaticCacheAtlasData.HasAllocation())
				{
					StaticCachedAtlasAllocator.Deallocate(ref item.StaticCacheAtlasData);
				}
				ShadowDataCacheMap.Remove(reference.LightID);
			}
		}
	}

	private void ExecutePhaseOne()
	{
		int num = -1;
		Span<LightDescriptor> span = LightDescriptorArray.AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref LightDescriptor reference = ref span[i];
			num++;
			if (reference.Shadows == LightShadows.None)
			{
				continue;
			}
			ShadowLightData actualLightData = LightDataFactory.Create(in reference);
			ShadowProcessData value = default(ShadowProcessData);
			if (ShadowDataCacheMap.TryGetValue(reference.LightID, out value.ShadowData))
			{
				EvaluateAtlasUpdateNeeds(in reference, in value.ShadowData, in actualLightData, out var needUpdateRenderData, out var needRender, out var needRenderCache);
				if (needRender)
				{
					DynamicAtlasAllocator.Deallocate(ref value.ShadowData.DynamicAtlasData);
					StatisticsWriter.IncrementRenderCandidateCounter();
				}
				if (needUpdateRenderData)
				{
					value.ShadowData.LightData = actualLightData;
					value.ShadowData.RenderDataValid = false;
					StatisticsWriter.IncrementUpdateRenderDataCandidateCounter();
				}
				if (StaticShadowCacheEnabled && needRenderCache && value.ShadowData.StaticCacheAtlasData.HasAllocation())
				{
					StaticCachedAtlasAllocator.Deallocate(ref value.ShadowData.StaticCacheAtlasData);
				}
			}
			else
			{
				value.ShadowData.LightData = actualLightData;
				StatisticsWriter.IncrementRenderCandidateCounter();
				StatisticsWriter.IncrementUpdateRenderDataCandidateCounter();
			}
			value.LightDescriptorIndex = num;
			value.ShadowData.LastVisibleFrameId = CurrentFrameId;
			ProcessDataList.Add(in value);
		}
	}

	private void ExecutePhaseTwo()
	{
		ShadowConstantBufferWriter shadowConstantBufferWriter = new ShadowConstantBufferWriter(in ConstantBufferReference);
		Span<LightDescriptor> span = LightDescriptorArray.AsSpan();
		int num = 0;
		Span<ShadowProcessData> span2 = ProcessDataList.AsArray().AsSpan();
		for (int i = 0; i < span2.Length; i++)
		{
			ref ShadowProcessData reference = ref span2[i];
			ref LightDescriptor reference2 = ref span[reference.LightDescriptorIndex];
			if (EnsureAtlasesDataValid(num, in reference2, ref reference))
			{
				shadowConstantBufferWriter.Write(num, in reference.ShadowData);
				reference2.ShadowDataIndex = num;
				if (++num >= 128)
				{
					StatisticsWriter.SetConstantBufferOverflowDetected();
					break;
				}
			}
		}
		if (StaticShadowCacheEnabled)
		{
			ShadowCopyCacheConstantBufferWriter shadowCopyCacheConstantBufferWriter = new ShadowCopyCacheConstantBufferWriter(in CachedShadowsCopyConstantBufferReference);
			int num2 = 0;
			Span<ShadowCacheCopyRequest> span3 = CacheCopyRequestList.AsArray().AsSpan();
			for (int i = 0; i < span3.Length; i++)
			{
				shadowCopyCacheConstantBufferWriter.Write(num2, in span3[i]);
				if (++num2 >= 128)
				{
					break;
				}
			}
		}
		StatisticsWriter.SetUseCandidateCounter(ProcessDataList.Length);
		StatisticsWriter.SetUseCounter(num);
	}

	private void UpdateShadowDataCache()
	{
		Span<LightDescriptor> span = LightDescriptorArray.AsSpan();
		Span<ShadowProcessData> span2 = ProcessDataList.AsArray().AsSpan();
		for (int i = 0; i < span2.Length; i++)
		{
			ref ShadowProcessData reference = ref span2[i];
			ref LightDescriptor reference2 = ref span[reference.LightDescriptorIndex];
			ShadowDataCacheMap[reference2.LightID] = reference.ShadowData;
		}
	}

	private void EvaluateAtlasUpdateNeeds(in LightDescriptor lightDescriptor, in ShadowData shadowData, in ShadowLightData actualLightData, out bool needUpdateRenderData, out bool needRender, out bool needRenderCache)
	{
		needUpdateRenderData = actualLightData.LightType == LightType.Directional || !shadowData.RenderDataValid || !actualLightData.NearlyEquals(in shadowData.LightData);
		needRender = (AlwaysUpdateShadows | needUpdateRenderData) || ShadowDistanceUpdateQualifier.ShouldUpdate(lightDescriptor.MeanZ, shadowData.LastRenderedFrameId);
		if (StaticShadowCacheEnabled)
		{
			needRenderCache = (AlwaysUpdateShadows | needUpdateRenderData) || (shadowData.LightData.CanBeCached && !shadowData.StaticCacheAtlasData.HasAllocation());
		}
		else
		{
			needRenderCache = false;
		}
	}

	private bool EnsureAtlasesDataValid(int shadowIndex, in LightDescriptor lightDescriptor, ref ShadowProcessData processData)
	{
		if (processData.ShadowData.DynamicAtlasData.HasAllocation())
		{
			return true;
		}
		if (!DynamicAtlasAllocator.Allocate(in processData.ShadowData.LightData, ref processData.ShadowData.DynamicAtlasData))
		{
			StatisticsWriter.SetDynamicAtlasOverflowDetected();
			return false;
		}
		if (!processData.ShadowData.RenderDataValid)
		{
			processData.ShadowData.RenderData = RenderDataFactory.Create(in processData.ShadowData.LightData);
			processData.ShadowData.RenderDataValid = true;
			StatisticsWriter.IncrementUpdateRenderDataCounter();
		}
		BuildRenderRequests(shadowIndex, in lightDescriptor, ref processData);
		StatisticsWriter.IncrementRenderCounter();
		processData.ShadowData.LastRenderedFrameId = CurrentFrameId;
		return true;
	}

	private void BuildRenderRequests(int shadowIndex, in LightDescriptor lightDescriptor, ref ShadowProcessData processData)
	{
		bool flag;
		if (processData.ShadowData.LightData.CanBeCached)
		{
			if (processData.ShadowData.StaticCacheAtlasData.HasAllocation())
			{
				flag = true;
			}
			else if (StaticCachedAtlasAllocator.Allocate(in processData.ShadowData.LightData, ref processData.ShadowData.StaticCacheAtlasData))
			{
				AddCacheAtlasRenderRequest(shadowIndex, in lightDescriptor, ref processData);
				flag = true;
			}
			else
			{
				StatisticsWriter.SetCachedAtlasOverflowDetected();
				flag = false;
			}
		}
		else
		{
			flag = false;
		}
		if (flag)
		{
			AddStaticAtlasCopyRequest(in processData);
			if (processData.ShadowData.LightData.AlwaysDrawDynamicShadowCasters)
			{
				AddDynamicAtlasRenderRequest(shadowIndex, in lightDescriptor, ref processData, ShadowObjectsFilter.DynamicOnly, needClear: false);
			}
		}
		else
		{
			AddDynamicAtlasRenderRequest(shadowIndex, in lightDescriptor, ref processData, ShadowObjectsFilter.AllObjects, needClear: true);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddCacheAtlasRenderRequest(int shadowIndex, in LightDescriptor lightDescriptor, ref ShadowProcessData processData)
	{
		ref NativeList<ShadowRenderRequest> renderRequestForCacheList = ref RenderRequestForCacheList;
		ShadowRenderRequest value = CreateRenderRequest(shadowIndex, ShadowObjectsFilter.StaticOnly, needClear: true, in lightDescriptor, in processData.ShadowData.RenderData, in processData.ShadowData.LightData, in processData.ShadowData.StaticCacheAtlasData);
		renderRequestForCacheList.Add(in value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddStaticAtlasCopyRequest(in ShadowProcessData processData)
	{
		int i = 0;
		for (int viewportCount = processData.ShadowData.LightData.ViewportCount; i < viewportCount; i++)
		{
			ref NativeList<ShadowCacheCopyRequest> cacheCopyRequestList = ref CacheCopyRequestList;
			ShadowCacheCopyRequest value = new ShadowCacheCopyRequest
			{
				DynamicAtlasScaleOffset = processData.ShadowData.DynamicAtlasData.ScaleOffsets[i],
				StaticAtlasScaleOffset = processData.ShadowData.StaticCacheAtlasData.ScaleOffsets[i]
			};
			cacheCopyRequestList.Add(in value);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddDynamicAtlasRenderRequest(int shadowIndex, in LightDescriptor lightDescriptor, ref ShadowProcessData processData, ShadowObjectsFilter objectsFilter, bool needClear)
	{
		ref NativeList<ShadowRenderRequest> renderRequestList = ref RenderRequestList;
		ShadowRenderRequest value = CreateRenderRequest(shadowIndex, objectsFilter, needClear, in lightDescriptor, in processData.ShadowData.RenderData, in processData.ShadowData.LightData, in processData.ShadowData.DynamicAtlasData);
		renderRequestList.Add(in value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ShadowRenderRequest CreateRenderRequest(int shadowIndex, ShadowObjectsFilter objectsFilter, bool needClear, in LightDescriptor lightDescriptor, in ShadowRenderData renderData, in ShadowLightData lightData, in ShadowAtlasData atlasData)
	{
		ShadowRenderRequest result = default(ShadowRenderRequest);
		result.ConstantBufferIndex = shadowIndex;
		result.VisibleLightIndex = lightDescriptor.LightUnsortedIndex;
		result.RenderData = renderData;
		result.ShadowMapViewports = atlasData.Viewports;
		result.FaceCount = lightData.FaceCount;
		result.LightType = lightData.LightType;
		result.DepthBias = lightData.DepthBias;
		result.ProjectionType = ((lightData.LightType != LightType.Directional) ? BatchCullingProjectionType.Perspective : BatchCullingProjectionType.Orthographic);
		result.ObjectsFilter = objectsFilter;
		result.NeedClear = needClear;
		return result;
	}
}
