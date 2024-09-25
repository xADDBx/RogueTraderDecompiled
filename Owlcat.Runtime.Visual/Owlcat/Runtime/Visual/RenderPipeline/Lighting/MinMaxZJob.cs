using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.Lighting;

[BurstCompile]
public struct MinMaxZJob : IJobFor
{
	public float4x4 WorldToViewMatrix;

	public NativeArray<LightDescriptor> LightDescriptors;

	[WriteOnly]
	public NativeArray<float> MeanZ;

	public void Execute(int index)
	{
		LightDescriptor value = LightDescriptors[index];
		if (value.VisibleLight.lightType == LightType.Directional)
		{
			value.MinZ = -10000f;
			value.MaxZ = -10000f;
			LightDescriptors[index] = value;
			MeanZ[index] = -10000f;
			return;
		}
		float4x4 float4x = value.VisibleLight.localToWorldMatrix;
		float3 xyz = float4x.c3.xyz;
		float3 xyz2 = math.mul(WorldToViewMatrix, math.float4(xyz, 1f)).xyz;
		value.MinZ = xyz2.z - value.VisibleLight.range;
		value.MaxZ = xyz2.z + value.VisibleLight.range;
		if (value.VisibleLight.lightType == LightType.Spot)
		{
			float num = math.radians(value.VisibleLight.spotAngle) * 0.5f;
			float num2 = math.cos(num);
			float num3 = value.VisibleLight.range * num2;
			float3 xyz3 = float4x.c2.xyz;
			float3 xyz4 = xyz + xyz3 * num3;
			float3 xyz5 = math.mul(WorldToViewMatrix, math.float4(xyz4, 1f)).xyz;
			float x = MathF.PI / 2f - num;
			float num4 = value.VisibleLight.range * num2 * math.sin(num) / math.sin(x);
			float3 @float = xyz5 - xyz2;
			float num5 = math.sqrt(1f - @float.z * @float.z / math.dot(@float, @float));
			if (0f - @float.z < num3 * num2)
			{
				value.MinZ = math.min(xyz2.z, xyz5.z - num5 * num4);
			}
			if (@float.z < num3 * num2)
			{
				value.MaxZ = math.max(xyz2.z, xyz5.z + num5 * num4);
			}
		}
		value.MinZ = math.max(value.MinZ, 0f);
		value.MaxZ = math.max(value.MaxZ, 0f);
		LightDescriptors[index] = value;
		MeanZ[index] = (value.MinZ + value.MaxZ) / 2f;
	}
}
