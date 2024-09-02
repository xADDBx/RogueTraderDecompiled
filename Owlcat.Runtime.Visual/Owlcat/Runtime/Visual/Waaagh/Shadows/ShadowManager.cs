using System.Text;
using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Collections;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

internal sealed class ShadowManager
{
	private const string kShadowMapAtlasName = "ShadowMapAtlas";

	private const string kCachedShadowMapName = "CachedShadowMapAtlas";

	private readonly WaaaghPipelineAsset m_Asset;

	private NativeHashMap<int, ShadowData> m_ShadowDataCacheMap;

	private NativeList<ShadowProcessData> m_ProcessDataList;

	private readonly StringBuilder m_MessageBuilder = new StringBuilder();

	private ShadowAtlas m_ShadowMapAtlas;

	private ShadowAtlas m_CachedShadowMapAtlas;

	private NativeReference<ShadowConstantBuffer> m_ShadowConstantBufferReference;

	private NativeReference<ShadowCopyCacheConstantBuffer> m_ShadowCopyCacheConstantBufferReference;

	private NativeList<ShadowRenderRequest> m_RenderRequestList;

	private NativeList<ShadowRenderRequest> m_RenderRequestForCacheList;

	private NativeList<ShadowCacheCopyRequest> m_ShadowCacheCopyRequestList;

	private NativeHashMap<int, PrecalculatedDirectionalShadowData> m_PrecalculatedDirectionalShadowDataMap;

	private NativeReference<ShadowStatistics> m_StatsisticsReference;

	private TetrahedronCalculator m_TetrahedronCalculator;

	private readonly Vector4[] m_PointLightClips;

	private readonly Vector4[] m_FaceVectors;

	public NativeList<ShadowRenderRequest> RenderRequests => m_RenderRequestList;

	public NativeList<ShadowRenderRequest> RenderRequestsForCache => m_RenderRequestForCacheList;

	public NativeList<ShadowCacheCopyRequest> ShadowCacheCopyRequests => m_ShadowCacheCopyRequestList;

	[CanBeNull]
	public ShadowAtlas ShadowMapAtlas => m_ShadowMapAtlas;

	[CanBeNull]
	public ShadowAtlas CachedShadowMapAtlas => m_CachedShadowMapAtlas;

	public ShadowManager(WaaaghPipelineAsset asset)
	{
		m_Asset = asset;
		m_ShadowDataCacheMap = new NativeHashMap<int, ShadowData>(128, Allocator.Persistent);
		m_ProcessDataList = new NativeList<ShadowProcessData>(128, Allocator.Persistent);
		m_ShadowConstantBufferReference = new NativeReference<ShadowConstantBuffer>(Allocator.Persistent);
		m_ShadowCopyCacheConstantBufferReference = new NativeReference<ShadowCopyCacheConstantBuffer>(Allocator.Persistent);
		m_RenderRequestList = new NativeList<ShadowRenderRequest>(128, Allocator.Persistent);
		m_RenderRequestForCacheList = new NativeList<ShadowRenderRequest>(128, Allocator.Persistent);
		m_ShadowCacheCopyRequestList = new NativeList<ShadowCacheCopyRequest>(128, Allocator.Persistent);
		m_TetrahedronCalculator = new TetrahedronCalculator(Allocator.Persistent);
		m_PrecalculatedDirectionalShadowDataMap = new NativeHashMap<int, PrecalculatedDirectionalShadowData>(4, Allocator.Persistent);
		m_StatsisticsReference = new NativeReference<ShadowStatistics>(Allocator.Persistent);
		m_PointLightClips = new Vector4[8]
		{
			CalculateLineEquationCoeffs(new Vector2(0f, 0f), new Vector2(-1f, 1f)),
			CalculateLineEquationCoeffs(new Vector2(1f, 1f), new Vector2(0f, 0f)),
			CalculateLineEquationCoeffs(new Vector2(0f, 0f), new Vector2(1f, 1f)),
			CalculateLineEquationCoeffs(new Vector2(1f, -1f), new Vector2(0f, 0f)),
			CalculateLineEquationCoeffs(new Vector2(0f, 0f), new Vector2(1f, -1f)),
			CalculateLineEquationCoeffs(new Vector2(-1f, -1f), new Vector2(0f, 0f)),
			CalculateLineEquationCoeffs(new Vector2(0f, 0f), new Vector2(-1f, -1f)),
			CalculateLineEquationCoeffs(new Vector2(-1f, 1f), new Vector2(0f, 0f))
		};
		m_FaceVectors = new Vector4[4]
		{
			m_TetrahedronCalculator.FaceVectors[0],
			m_TetrahedronCalculator.FaceVectors[1],
			m_TetrahedronCalculator.FaceVectors[2],
			m_TetrahedronCalculator.FaceVectors[3]
		};
	}

	public void Dispose()
	{
		m_ShadowMapAtlas?.Dispose();
		m_CachedShadowMapAtlas?.Dispose();
		m_ProcessDataList.Dispose();
		m_ShadowDataCacheMap.Dispose();
		m_ShadowConstantBufferReference.Dispose();
		m_ShadowCopyCacheConstantBufferReference.Dispose();
		m_RenderRequestList.Dispose();
		m_RenderRequestForCacheList.Dispose();
		m_ShadowCacheCopyRequestList.Dispose();
		m_TetrahedronCalculator.Dispose();
		m_PrecalculatedDirectionalShadowDataMap.Dispose();
		m_StatsisticsReference.Dispose();
	}

	public JobHandle ScheduleSetupJobs(ref NativeArray<LightDescriptor> lightDescriptorArray, ref RenderingData renderingData, JobHandle dependency)
	{
		m_PrecalculatedDirectionalShadowDataMap.Clear();
		m_ProcessDataList.Clear();
		m_RenderRequestList.Clear();
		m_RenderRequestForCacheList.Clear();
		m_ShadowCacheCopyRequestList.Clear();
		m_StatsisticsReference.Value = default(ShadowStatistics);
		ShadowSettings shadowSettings = m_Asset.ShadowSettings;
		UpdateShadowMapAtlases(shadowSettings);
		if (shadowSettings.ShadowQuality == ShadowQuality.Disable)
		{
			return dependency;
		}
		PrecalculatedDirectionalShadowDataFactory.Populate(in renderingData, m_PrecalculatedDirectionalShadowDataMap);
		int frameId = renderingData.TimeData.FrameId;
		ShadowLightDataFactory lightDataFactory = new ShadowLightDataFactory(in renderingData);
		ShadowAtlasViewportAllocator dynamicAtlasAllocator = new ShadowAtlasViewportAllocator(m_ShadowMapAtlas.Allocator);
		ShadowAtlasViewportAllocator staticCachedAtlasAllocator = default(ShadowAtlasViewportAllocator);
		if (renderingData.ShadowData.StaticShadowsCacheEnabled)
		{
			staticCachedAtlasAllocator = new ShadowAtlasViewportAllocator(m_CachedShadowMapAtlas.Allocator);
		}
		ShadowDistanceUpdateQualifier shadowDistanceUpdateQualifier = new ShadowDistanceUpdateQualifier(renderingData, frameId);
		ShadowRenderDataFactory renderDataFactory = new ShadowRenderDataFactory(m_PrecalculatedDirectionalShadowDataMap, m_TetrahedronCalculator, SystemInfo.usesReversedZBuffer);
		ShadowStatisticsWriter statisticsWriter = new ShadowStatisticsWriter(m_StatsisticsReference);
		ShadowJob jobData = default(ShadowJob);
		jobData.CurrentFrameId = frameId;
		jobData.StaticShadowCacheEnabled = renderingData.ShadowData.StaticShadowsCacheEnabled;
		jobData.LightDescriptorArray = lightDescriptorArray;
		jobData.ShadowDataCacheMap = m_ShadowDataCacheMap;
		jobData.ProcessDataList = m_ProcessDataList;
		jobData.RenderRequestList = m_RenderRequestList;
		jobData.RenderRequestForCacheList = m_RenderRequestForCacheList;
		jobData.CacheCopyRequestList = m_ShadowCacheCopyRequestList;
		jobData.ConstantBufferReference = m_ShadowConstantBufferReference;
		jobData.CachedShadowsCopyConstantBufferReference = m_ShadowCopyCacheConstantBufferReference;
		jobData.LightDataFactory = lightDataFactory;
		jobData.DynamicAtlasAllocator = dynamicAtlasAllocator;
		jobData.StaticCachedAtlasAllocator = staticCachedAtlasAllocator;
		jobData.ShadowDistanceUpdateQualifier = shadowDistanceUpdateQualifier;
		jobData.RenderDataFactory = renderDataFactory;
		jobData.StatisticsWriter = statisticsWriter;
		jobData.AlwaysUpdateShadows = FrameDebugger.enabled;
		return jobData.Schedule(dependency);
	}

	public void FinishSetup(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CreateShadowRenderLists(context, in renderingData);
		ReportStatistics(in renderingData);
	}

	private void UpdateShadowMapAtlases(ShadowSettings settings)
	{
		if (settings.ShadowQuality != 0)
		{
			if (m_ShadowMapAtlas != null)
			{
				m_ShadowMapAtlas.Resolution = settings.AtlasSize;
			}
			else
			{
				m_ShadowMapAtlas = new ShadowAtlas("ShadowMapAtlas", settings.AtlasSize);
			}
		}
		else if (m_ShadowMapAtlas != null)
		{
			m_ShadowMapAtlas.Dispose();
			m_ShadowMapAtlas = null;
		}
		if (settings.ShadowQuality != 0 && settings.StaticShadowsCacheEnabled)
		{
			if (m_CachedShadowMapAtlas != null)
			{
				m_CachedShadowMapAtlas.Resolution = settings.CacheAtlasSize;
			}
			else
			{
				m_CachedShadowMapAtlas = new ShadowAtlas("CachedShadowMapAtlas", settings.CacheAtlasSize);
			}
		}
		else if (m_CachedShadowMapAtlas != null)
		{
			m_CachedShadowMapAtlas.Dispose();
			m_CachedShadowMapAtlas = null;
		}
	}

	private void CreateShadowRenderLists(ScriptableRenderContext context, in RenderingData renderingData)
	{
		bool supportsLightLayers = WaaaghPipeline.Asset.SupportsLightLayers;
		CreateRenderListsForRequestsList(m_RenderRequestForCacheList, context, in renderingData, supportsLightLayers);
		CreateRenderListsForRequestsList(m_RenderRequestList, context, in renderingData, supportsLightLayers);
	}

	private unsafe void CreateRenderListsForRequestsList(NativeList<ShadowRenderRequest> requestList, ScriptableRenderContext context, in RenderingData renderingData, bool useRenderingLayerMask)
	{
		ShadowRenderRequest* unsafePtr = requestList.GetUnsafePtr();
		int length = requestList.Length;
		for (int i = 0; i < length; i++)
		{
			ref ShadowRenderRequest reference = ref unsafePtr[i];
			for (int j = 0; j < reference.FaceCount; j++)
			{
				ShadowDrawingSettings settings = new ShadowDrawingSettings(renderingData.CullingResults, reference.VisibleLightIndex, reference.ProjectionType);
				settings.useRenderingLayerMaskTest = useRenderingLayerMask;
				settings.objectsFilter = reference.ObjectsFilter;
				settings.splitData = reference.RenderData.SplitDataArray[j];
				reference.RendererListArray[j] = context.CreateShadowRendererList(ref settings);
			}
		}
	}

	private void ReportStatistics(in RenderingData renderingData)
	{
	}

	private static Vector3 CalculateLineEquationCoeffs(Vector2 p1, Vector2 p2)
	{
		return new Vector3(p1.y - p2.y, p2.x - p1.x, p1.x * p2.y - p2.x * p1.y);
	}

	public void PushShadowConstantBuffer(CommandBuffer cmd)
	{
		ConstantBuffer.PushGlobal(cmd, in UnsafeCollectionExtensions.AsRef(in m_ShadowConstantBufferReference), ShaderPropertyId.ShadowConstantBuffer);
		ConstantBuffer.PushGlobal(cmd, in UnsafeCollectionExtensions.AsRef(in m_ShadowCopyCacheConstantBufferReference), ShaderPropertyId.ShadowCopyCacheConstantBuffer);
		cmd.SetGlobalVectorArray(ShaderPropertyId._Clips, m_PointLightClips);
		cmd.SetGlobalVectorArray(ShaderPropertyId._FaceVectors, m_FaceVectors);
	}
}
