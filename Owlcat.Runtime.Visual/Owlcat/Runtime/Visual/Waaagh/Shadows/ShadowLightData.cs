using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct ShadowLightData
{
	private const float kCachedShadowTranslationThreshold = 0.01f;

	private const float kCachedShadowTranslationThresholdSq = 0.0001f;

	private const float kCachedShadowRotationThreshold = 0.99999f;

	private const float kCachedSpotAngleThreshold = 0.1f;

	private const float kCachedRangeThreshold = 0.01f;

	private const float kCachedDepthBiasThreshold = 0.01f;

	private const float kCachedNormalBiasThreshold = 0.01f;

	private const float kCachedNearPlaneThreshold = 0.01f;

	public int LightId;

	public LightType LightType;

	public float4x4 LocalToWorldMatrix;

	public float Range;

	public float SpotAngle;

	public float ShadowNearPlane;

	public float DepthBias;

	public float NormalBias;

	public LightShadows Shadows;

	public int Resolution;

	public int FaceCount;

	public int ViewportCount;

	public bool CanBeCached;

	public bool AlwaysDrawDynamicShadowCasters;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool NearlyEquals(in ShadowLightData other)
	{
		if (LightType == other.LightType && Shadows == other.Shadows && ViewportCount == other.ViewportCount && FaceCount == other.FaceCount && Resolution == other.Resolution && CanBeCached == other.CanBeCached && ArePositionNearlyEquals(in LocalToWorldMatrix, in other.LocalToWorldMatrix) && AreNearlyEquals(Range, other.Range, 0.01f) && AreNearlyEquals(ShadowNearPlane, other.ShadowNearPlane, 0.01f) && AreNearlyEquals(DepthBias, other.DepthBias, 0.01f) && AreNearlyEquals(NormalBias, other.NormalBias, 0.01f))
		{
			if (LightType == LightType.Spot)
			{
				return SpotLightDataNearlyEquals(in other);
			}
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool SpotLightDataNearlyEquals(in ShadowLightData other)
	{
		if (AreNearlyEquals(SpotAngle, other.SpotAngle, 0.1f))
		{
			return AreRotationNearlyEquals(in LocalToWorldMatrix, in other.LocalToWorldMatrix);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool AreNearlyEquals(float a, float b, float epsilon)
	{
		return math.distance(a, b) <= epsilon;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool ArePositionNearlyEquals(in float4x4 a, in float4x4 b)
	{
		float3 xyz = a.c3.xyz;
		float3 xyz2 = b.c3.xyz;
		return math.distancesq(xyz, xyz2) <= 0.0001f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool AreRotationNearlyEquals(in float4x4 a, in float4x4 b)
	{
		float3 x = math.rotate(a, new float3(0f, 0f, 1f));
		float3 y = math.rotate(b, new float3(0f, 0f, 1f));
		return math.dot(x, y) >= 0.99999f;
	}
}
