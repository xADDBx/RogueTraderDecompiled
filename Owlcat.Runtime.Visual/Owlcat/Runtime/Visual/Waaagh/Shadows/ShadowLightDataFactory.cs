using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

internal struct ShadowLightDataFactory
{
	private const int kMinPunctualLightResolution = 128;

	private const int kLightTypeCount = 5;

	private const int kPointLightFaceCount = 4;

	private readonly int2 m_ScreenResolution;

	private readonly ShadowResolutionSettings m_SpotLightResolution;

	private readonly ShadowResolutionSettings m_PointLightResolution;

	private readonly int m_DirectionalLightResolutionMax;

	private unsafe fixed int m_LightTypeToFaceCountMap[5];

	private unsafe fixed int m_LightTypeToViewportCountMap[5];

	public unsafe ShadowLightDataFactory(in RenderingData renderingData)
	{
		m_ScreenResolution = new int2(renderingData.CameraData.CameraTargetDescriptor.width, renderingData.CameraData.CameraTargetDescriptor.height);
		m_SpotLightResolution = renderingData.ShadowData.SpotLightResolution;
		m_PointLightResolution = renderingData.ShadowData.PointLightResolution;
		m_DirectionalLightResolutionMax = (int)renderingData.ShadowData.DirectionalLightCascadeResolution;
		m_LightTypeToFaceCountMap[0] = 1;
		m_LightTypeToFaceCountMap[1] = renderingData.ShadowData.DirectionalLightCascades.Count;
		m_LightTypeToFaceCountMap[2] = 4;
		m_LightTypeToFaceCountMap[3] = 0;
		m_LightTypeToFaceCountMap[4] = 0;
		m_LightTypeToViewportCountMap[0] = 1;
		m_LightTypeToViewportCountMap[1] = renderingData.ShadowData.DirectionalLightCascades.Count;
		m_LightTypeToViewportCountMap[2] = 1;
		m_LightTypeToViewportCountMap[3] = 0;
		m_LightTypeToViewportCountMap[4] = 0;
	}

	[BurstCompile]
	public ShadowLightData Create(in LightDescriptor lightDescriptor)
	{
		ShadowLightData result = default(ShadowLightData);
		result.LightId = lightDescriptor.LightID;
		result.LightType = lightDescriptor.VisibleLight.lightType;
		result.LocalToWorldMatrix = lightDescriptor.VisibleLight.localToWorldMatrix;
		result.Range = lightDescriptor.VisibleLight.range;
		result.SpotAngle = lightDescriptor.VisibleLight.spotAngle;
		result.ShadowNearPlane = lightDescriptor.ShadowNearPlane;
		result.Shadows = lightDescriptor.Shadows;
		result.Resolution = GetShadowMapResolution(in lightDescriptor);
		result.FaceCount = GetShadowMapFaceCount(lightDescriptor.VisibleLight.lightType);
		result.ViewportCount = GetShadowMapViewportCount(lightDescriptor.VisibleLight.lightType);
		result.DepthBias = lightDescriptor.ShadowDepthBias;
		result.NormalBias = lightDescriptor.ShadowNormalBias;
		result.CanBeCached = lightDescriptor.ShadowsCanBeCached;
		result.AlwaysDrawDynamicShadowCasters = lightDescriptor.ShadowmapAlwaysDrawDynamicShadowCasters;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	private unsafe int GetShadowMapFaceCount(LightType lightType)
	{
		return m_LightTypeToFaceCountMap[(int)lightType];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	private unsafe int GetShadowMapViewportCount(LightType lightType)
	{
		return m_LightTypeToViewportCountMap[(int)lightType];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	private int GetShadowMapResolution(in LightDescriptor lightDescriptor)
	{
		return lightDescriptor.VisibleLight.lightType switch
		{
			LightType.Spot => GetPunctualLightShadowMapResolution(in lightDescriptor, in m_SpotLightResolution), 
			LightType.Directional => m_DirectionalLightResolutionMax, 
			LightType.Point => GetPunctualLightShadowMapResolution(in lightDescriptor, in m_PointLightResolution), 
			_ => 0, 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetPunctualLightShadowMapResolution(in LightDescriptor lightDescriptor, in ShadowResolutionSettings resolutionSettings)
	{
		Rect screenRect = lightDescriptor.VisibleLight.screenRect;
		int num = Mathf.ClosestPowerOfTwo((int)math.max((float)m_ScreenResolution.x * screenRect.width, (float)m_ScreenResolution.y * screenRect.height));
		if (num < 128)
		{
			return 128;
		}
		return math.min(num, (ShadowResolutionTier)(lightDescriptor.ShadowmapResolution switch
		{
			LightShadowmapResolution.Default => (int)resolutionSettings.DefaultTier, 
			LightShadowmapResolution.Low => 0, 
			LightShadowmapResolution.Medium => 1, 
			LightShadowmapResolution.High => 2, 
			LightShadowmapResolution.Ultra => 3, 
			_ => (int)resolutionSettings.DefaultTier, 
		}) switch
		{
			ShadowResolutionTier.Low => (int)resolutionSettings.Low, 
			ShadowResolutionTier.Medium => (int)resolutionSettings.Medium, 
			ShadowResolutionTier.High => (int)resolutionSettings.High, 
			ShadowResolutionTier.Ultra => (int)resolutionSettings.Ultra, 
			_ => (int)resolutionSettings.Low, 
		});
	}
}
