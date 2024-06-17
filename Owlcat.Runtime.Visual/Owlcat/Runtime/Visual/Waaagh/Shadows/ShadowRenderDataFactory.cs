using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct ShadowRenderDataFactory
{
	private const float kFov0 = 143.9857f;

	private const float kFov1 = 125.26439f;

	private const float kFilterWidth = 5f;

	private const float kPointLightFilterMultiplier = 5f;

	private static readonly float4 s_NullSphere = new float4(0f, 0f, 0f, float.NegativeInfinity);

	private NativeHashMap<int, PrecalculatedDirectionalShadowData> m_PrecalculatedDirectionalShadowDataMap;

	private TetrahedronCalculator m_TetrahedronCalculator;

	private readonly bool m_ReverseZ;

	public ShadowRenderDataFactory(NativeHashMap<int, PrecalculatedDirectionalShadowData> precalculatedDirectionalShadowDataMap, TetrahedronCalculator tetrahedronCalculator, bool reverseZ)
	{
		m_PrecalculatedDirectionalShadowDataMap = precalculatedDirectionalShadowDataMap;
		m_TetrahedronCalculator = tetrahedronCalculator;
		m_ReverseZ = reverseZ;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ShadowRenderData Create(in ShadowLightData lightData)
	{
		return lightData.LightType switch
		{
			LightType.Spot => CreateSpotLightRenderData(in lightData), 
			LightType.Directional => CreateDirectionalLightRenderData(in lightData), 
			LightType.Point => CreatePointLightRenderData(in lightData), 
			_ => throw new InvalidOperationException("Unsupported light type"), 
		};
	}

	private ShadowRenderData CreateDirectionalLightRenderData(in ShadowLightData lightData)
	{
		PrecalculatedDirectionalShadowData precalculatedDirectionalShadowData = m_PrecalculatedDirectionalShadowDataMap[lightData.LightId];
		FixedArray4<float> gPUDepthBiasArray = default(FixedArray4<float>);
		FixedArray4<float> gPUNormalBiasArray = default(FixedArray4<float>);
		for (int i = 0; i < lightData.FaceCount; i++)
		{
			float frustumSize = precalculatedDirectionalShadowData.FrustumSizeArray[i];
			CalculateGpuDepthNormalBias(in lightData, frustumSize, out var _, out var gpuNormalBias);
			gPUDepthBiasArray[i] = 0f;
			gPUNormalBiasArray[i] = gpuNormalBias;
		}
		ShadowRenderData result = default(ShadowRenderData);
		result.SplitDataArray = precalculatedDirectionalShadowData.SplitDataArray;
		result.FaceDataArray = precalculatedDirectionalShadowData.FaceDataArray;
		result.GPUDepthBiasArray = gPUDepthBiasArray;
		result.GPUNormalBiasArray = gPUNormalBiasArray;
		return result;
	}

	private ShadowRenderData CreatePointLightRenderData(in ShadowLightData lightData)
	{
		ShadowRenderData result = default(ShadowRenderData);
		float filterWidth = 25f;
		float normalBiasMax = 0.5f;
		float num = CalcGuardAnglePerspective(143.9857f, lightData.Resolution, filterWidth, normalBiasMax, 36.014297f);
		float num2 = CalcGuardAnglePerspective(125.26439f, lightData.Resolution, filterWidth, normalBiasMax, 54.73561f);
		m_TetrahedronCalculator.TempPointProjMatrices[0] = CreatePerspectiveMatrix(new float2(143.9857f + num, 125.26439f + num2), lightData.ShadowNearPlane * 0.5f, lightData.Range, m_ReverseZ);
		m_TetrahedronCalculator.TempPointProjMatrices[1] = CreatePerspectiveMatrix(new float2(125.26439f + num2, 143.9857f + num), lightData.ShadowNearPlane * 0.5f, lightData.Range, m_ReverseZ);
		float3 lightPos = lightData.LocalToWorldMatrix.c3.xyz;
		float4x4 b = float4x4.Translate(-lightPos);
		for (int i = 0; i < 4; i++)
		{
			float4x4 b2 = math.mul(m_TetrahedronCalculator.PointScaleMatrices[i], math.mul(m_TetrahedronCalculator.TetrahedronMatrices[i], b));
			int index = i & 1;
			float4x4 a = m_TetrahedronCalculator.TempPointProjMatrices[index];
			float4x4 worldToShadow = math.mul(m_TetrahedronCalculator.PointLightTexMatrices[i], math.mul(a, b2));
			float4 @float = new Vector4(lightPos.x, lightPos.y, lightPos.z, lightData.Range);
			ShadowSplitData shadowSplitData = default(ShadowSplitData);
			shadowSplitData.cullingSphere = @float;
			ShadowSplitData shadowSplitData2 = shadowSplitData;
			m_TetrahedronCalculator.GetFacePlanes(in lightPos, i, ref shadowSplitData2);
			result.SplitDataArray[i] = shadowSplitData2;
			float4 faceDirection = -m_TetrahedronCalculator.FaceVectors[i];
			@float.w *= @float.w;
			result.FaceDataArray[i] = new ShadowFaceData
			{
				CullingSphere = @float,
				FaceDirection = faceDirection,
				WorldToShadow = worldToShadow
			};
		}
		float frustumSize = math.tan(math.radians(71.99285f)) * lightData.Range;
		CalculateGpuDepthNormalBias(in lightData, frustumSize, out var gpuDepthBias, out var gpuNormalBias);
		gpuDepthBias = 0f;
		result.GPUDepthBiasArray = new FixedArray4<float>(in gpuDepthBias);
		result.GPUNormalBiasArray = new FixedArray4<float>(in gpuNormalBias);
		return result;
	}

	private ShadowRenderData CreateSpotLightRenderData(in ShadowLightData lightData)
	{
		float4x4 identity = float4x4.identity;
		identity[2][2] = -1f;
		float4x4 b = math.mul(identity, math.inverse(lightData.LocalToWorldMatrix));
		float range = lightData.Range;
		float shadowNearPlane = lightData.ShadowNearPlane;
		float normalBiasMax = 4f;
		float num = CalcGuardAnglePerspective(lightData.SpotAngle, lightData.Resolution, 5f, normalBiasMax, 180f - lightData.SpotAngle);
		float num2 = math.radians(lightData.SpotAngle + num);
		float4x4 projectionMatrix = float4x4.PerspectiveFov(num2, 1f, shadowNearPlane, range);
		float4x4 gPUProjectionMatrix = GetGPUProjectionMatrix(in projectionMatrix, invertY: true, m_ReverseZ);
		ShadowFaceData item = new ShadowFaceData
		{
			CullingSphere = s_NullSphere,
			FaceDirection = -lightData.LocalToWorldMatrix.c2,
			WorldToShadow = math.mul(gPUProjectionMatrix, b)
		};
		float frustumSize = math.tan(num2 * 0.5f) * lightData.Range;
		CalculateGpuDepthNormalBias(in lightData, frustumSize, out var gpuDepthBias, out var gpuNormalBias);
		float3 xyz = lightData.LocalToWorldMatrix.c3.xyz;
		ShadowRenderData result = default(ShadowRenderData);
		result.SplitDataArray = new FixedArray4<ShadowSplitData>
		{
			item0 = new ShadowSplitData
			{
				cullingPlaneCount = 0,
				cullingSphere = new Vector4(xyz.x, xyz.y, xyz.z, lightData.Range)
			}
		};
		result.FaceDataArray = new FixedArray4<ShadowFaceData>
		{
			item0 = item
		};
		gpuDepthBias = 0f;
		result.GPUDepthBiasArray = new FixedArray4<float>(in gpuDepthBias);
		result.GPUNormalBiasArray = new FixedArray4<float>(in gpuNormalBias);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float CalcGuardAnglePerspective(float angleInDeg, float resolution, float filterWidth, float normalBiasMax, float guardAngleMaxInDeg)
	{
		float num = math.radians(angleInDeg * 0.5f);
		float num2 = 2f / resolution;
		float num3 = math.cos(num) * num2;
		float num4 = math.atan(normalBiasMax * num3 * 1.4142135f);
		num3 = math.tan(num + num4) * num2;
		num4 = math.degrees(math.atan((resolution + math.ceil(filterWidth)) * num3 * 0.5f) * 2f) - angleInDeg;
		num4 *= 2f;
		if (!(num4 < guardAngleMaxInDeg))
		{
			return guardAngleMaxInDeg;
		}
		return num4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float4x4 CreatePerspectiveMatrix(float2 verticalFovInDeg, float near, float far, bool reverseZ)
	{
		verticalFovInDeg = math.radians(verticalFovInDeg);
		float4x4 projectionMatrix = float4x4.PerspectiveFov(verticalFovInDeg.y, 1f, near, far);
		float2 @float = 1f / math.tan(verticalFovInDeg * 0.5f);
		projectionMatrix[0][0] = @float.x;
		projectionMatrix[1][1] = @float.y;
		return GetGPUProjectionMatrix(in projectionMatrix, invertY: true, reverseZ);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float4x4 GetGPUProjectionMatrix(in float4x4 projectionMatrix, bool invertY, bool reverseZ)
	{
		float4x4 v = math.transpose(projectionMatrix);
		if (invertY)
		{
			v.c1 = -v.c1;
		}
		float num = (reverseZ ? (-0.5f) : 0.5f);
		v.c2 = v.c2 * num + v.c3 * 0.5f;
		return math.transpose(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void CalculateGpuDepthNormalBias(in ShadowLightData lightData, float frustumSize, out float gpuDepthBias, out float gpuNormalBias)
	{
		float num = frustumSize / (float)lightData.Resolution;
		gpuDepthBias = (0f - lightData.DepthBias) * num;
		gpuNormalBias = (0f - lightData.NormalBias) * num;
		if (lightData.Shadows == LightShadows.Soft)
		{
			float num2 = 2.5f;
			gpuDepthBias *= num2;
			gpuNormalBias *= num2;
		}
	}
}
