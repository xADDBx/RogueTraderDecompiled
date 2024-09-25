using System;
using Owlcat.Runtime.Visual.Lighting;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Lighting;

[BurstCompile]
public struct ExtractLightDataJob : IJobFor
{
	public float4x4 WorldToViewMatrix;

	[ReadOnly]
	public NativeArray<LightDescriptor> LightDescriptors;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> LightData;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> LightVolumeData;

	public void Execute(int index)
	{
		int num = 1024 + index;
		int num2 = 2048 + index;
		int index2 = 3072 + index;
		int index3 = num;
		int index4 = num2;
		float4 value = 0;
		float4 value2 = new float4(1f, 0f, 0f, 1f);
		float4 @float = 0;
		float4 value3 = 0;
		float4 value4 = 0;
		float4 value5 = 0;
		float4 value6 = 0;
		LightDescriptor lightDescriptor = LightDescriptors[index];
		value6.x = lightDescriptor.MinZ;
		value6.y = lightDescriptor.MaxZ;
		value6.z = lightDescriptor.VolumetricIntensity;
		float4x4 float4x = lightDescriptor.VisibleLight.localToWorldMatrix;
		if (lightDescriptor.VisibleLight.lightType == LightType.Directional)
		{
			value.xyz = -float4x.c2.xyz;
		}
		else
		{
			value.xyz = float4x.c3.xyz;
		}
		uint x = PackData(lightDescriptor.LightFalloffType, lightDescriptor.SnapSpecularToInnerRadius, lightDescriptor.ShadowDataIndex, lightDescriptor.ShadowmaskChannel, lightDescriptor.LightLayerMask, lightDescriptor.VolumetricLighting, lightDescriptor.VolumetricShadows, lightDescriptor.LightCookieIndex);
		value.w = math.asfloat(x);
		Color finalColor = lightDescriptor.VisibleLight.finalColor;
		@float = new float4(finalColor.r, finalColor.g, finalColor.b, lightDescriptor.ShadowStrength);
		float w = 0f;
		if (lightDescriptor.VisibleLight.lightType != LightType.Directional)
		{
			LightFalloffType num3 = ((!lightDescriptor.IsBaked) ? lightDescriptor.LightFalloffType : LightFalloffType.InverseSquared);
			w = (lightDescriptor.IsBaked ? 0f : lightDescriptor.InnerRadius);
			w = math.clamp(w, 0f, 0.999f);
			if (num3 == LightFalloffType.InverseSquared)
			{
				float num4 = lightDescriptor.VisibleLight.range * lightDescriptor.VisibleLight.range;
				float num5 = w * w * num4 - num4;
				float num6 = 1f / num5;
				float y = (0f - num4) / num5;
				float num7 = 1f / math.max(0.0001f, lightDescriptor.VisibleLight.range * lightDescriptor.VisibleLight.range);
				value2.x = ((w > 0f) ? num6 : num7);
				value2.y = y;
			}
			else
			{
				float x2 = 1f / math.max(0.0001f, lightDescriptor.VisibleLight.range * lightDescriptor.VisibleLight.range);
				float y2 = 25f / math.max(0.0001f, lightDescriptor.VisibleLight.range * lightDescriptor.VisibleLight.range);
				value2.x = x2;
				value2.y = y2;
			}
			w *= lightDescriptor.VisibleLight.range;
			value4 = math.mul(WorldToViewMatrix, new float4(value.xyz, 1f));
			value4.w = lightDescriptor.VisibleLight.range;
		}
		if (lightDescriptor.VisibleLight.lightType == LightType.Spot)
		{
			value3.xyz = -float4x.c2.xyz;
			float num8 = Mathf.Cos(MathF.PI / 180f * lightDescriptor.VisibleLight.spotAngle * 0.5f);
			float num9 = ((!(lightDescriptor.InnerSpotAngle > 0f)) ? math.cos(2f * math.atan(math.tan(lightDescriptor.VisibleLight.spotAngle * 0.5f * (MathF.PI / 180f)) * 46f / 64f) * 0.5f) : math.cos(lightDescriptor.InnerSpotAngle * (MathF.PI / 180f) * 0.5f));
			float num10 = Mathf.Max(0.001f, num9 - num8);
			float num11 = 1f / num10;
			float w2 = (0f - num8) * num11;
			value2.z = num11;
			value2.w = w2;
			value5.xyz = math.mul((float3x3)WorldToViewMatrix, -value3.xyz);
			value5.w = math.radians(lightDescriptor.VisibleLight.spotAngle * 0.5f);
		}
		else
		{
			value5 = new float4(0f, 0f, 0f, -1f);
		}
		value3.w = w;
		LightData[index] = value;
		LightData[num] = value2;
		LightData[num2] = @float;
		LightData[index2] = value3;
		LightVolumeData[index] = value4;
		LightVolumeData[index3] = value5;
		LightVolumeData[index4] = value6;
	}

	private uint PackData(LightFalloffType lightFalloffType, bool snapSpecularToInnerRadius, int shadowDataIndex, int shadowmaskChannel, uint lightLayerMask, bool volumetricLighting, bool volumetricShadows, int lightCookieIndex)
	{
		uint num = 0u;
		num |= ((lightFalloffType == LightFalloffType.Legacy) ? 1u : 0u);
		num |= (snapSpecularToInnerRadius ? 2u : 0u);
		if (shadowDataIndex > -1 && shadowDataIndex < 128)
		{
			num |= ((shadowDataIndex > -1) ? 4u : 0u);
			if (shadowDataIndex > -1)
			{
				num |= (uint)(shadowDataIndex << 3);
			}
		}
		num |= ((shadowmaskChannel > -1) ? 1024u : 0u);
		if (shadowmaskChannel > -1)
		{
			num |= (uint)(shadowmaskChannel << 11);
		}
		num |= lightLayerMask << 13;
		num |= (volumetricLighting ? 2097152u : 0u);
		num |= (volumetricShadows ? 4194304u : 0u);
		if (lightCookieIndex > -1 && lightCookieIndex < 128)
		{
			num |= ((lightCookieIndex > -1) ? 8388608u : 0u);
			if (lightCookieIndex > -1)
			{
				num |= (uint)(lightCookieIndex << 24);
			}
		}
		return num;
	}
}
