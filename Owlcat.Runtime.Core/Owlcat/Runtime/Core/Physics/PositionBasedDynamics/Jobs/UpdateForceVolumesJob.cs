using System;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;

[BurstCompile]
public struct UpdateForceVolumesJob : IJobParallelFor
{
	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> ForceVolumeEnumPackedValues;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4x2> ForceVolumeParameters;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ForceVolumeAabbMin;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ForceVolumeAabbMax;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4x3> ForceVolumeEmissionParameters;

	[ReadOnly]
	public NativeArray<int> EnumPackedValues;

	[ReadOnly]
	public NativeArray<float4x4> LocalToWorldVolumeMatrices;

	[ReadOnly]
	public NativeArray<float4> VolumeParameters0;

	[ReadOnly]
	public NativeArray<float4x4> LocalToWorldEmitterMatrices;

	[ReadOnly]
	public NativeArray<float3> EmitterDirection;

	[ReadOnly]
	public NativeArray<float> EmitterDirectionLerp;

	[ReadOnly]
	public NativeArray<float> EmitterIntensity;

	[ReadOnly]
	public int BatchOffset;

	[ReadOnly]
	public int AabbOffset;

	public void Execute(int index)
	{
		int num = index + BatchOffset;
		int num2 = EnumPackedValues[index];
		ForceVolume.UnpackEnumValues(num2, out var volumeType, out var emissionType, out var axis, out var _);
		float4 volumeParameters = VolumeParameters0[index];
		float4x4 localToWorld = LocalToWorldVolumeMatrices[index];
		float3 aabbMin = default(float3);
		float3 aabbMax = default(float3);
		float4 forceVolumeParameters = default(float4);
		float4 forceVolumeParameters2 = default(float4);
		switch (volumeType)
		{
		case ForceVolumeType.Sphere:
			UpdateSphere(ref volumeParameters, ref localToWorld, ref aabbMin, ref aabbMax, ref forceVolumeParameters);
			break;
		case ForceVolumeType.Cylinder:
			UpdateCylinder(ref volumeParameters, ref localToWorld, ref aabbMin, ref aabbMax, ref forceVolumeParameters, ref forceVolumeParameters2);
			break;
		case ForceVolumeType.Cone:
			UpdateCone(ref volumeParameters, ref localToWorld, ref aabbMin, ref aabbMax, ref forceVolumeParameters, ref forceVolumeParameters2);
			break;
		}
		ForceVolumeEnumPackedValues[num] = num2;
		ForceVolumeParameters[num] = new float4x2(forceVolumeParameters, forceVolumeParameters2);
		ForceVolumeAabbMin[num + AabbOffset] = aabbMin;
		ForceVolumeAabbMax[num + AabbOffset] = aabbMax;
		float4x4 float4x = LocalToWorldEmitterMatrices[index];
		float4 c = new float4(float4x.c3.xyz, EmitterIntensity[index]);
		float4 c2 = new float4(math.normalize(EmitterDirection[index]), EmitterDirectionLerp[index]);
		float4 c3 = new float4(0f, 1f, 0f, 0f);
		if (emissionType == ForceEmissionType.Axis || emissionType == ForceEmissionType.Vortex)
		{
			switch (axis)
			{
			case AxisDirection.AxisY:
				c3.xyz = math.normalize(float4x.c1.xyz);
				break;
			case AxisDirection.AxisZ:
				c3.xyz = math.normalize(float4x.c2.xyz);
				break;
			case AxisDirection.AxisX:
				c3.xyz = math.normalize(float4x.c0.xyz);
				break;
			}
		}
		ForceVolumeEmissionParameters[num] = new float4x3(c, c2, c3);
	}

	private void UpdateSphere(ref float4 volumeParameters0, ref float4x4 localToWorld, ref float3 aabbMin, ref float3 aabbMax, ref float4 forceVolumeParameters0)
	{
		forceVolumeParameters0 = math.mul(localToWorld, new float4(volumeParameters0.xyz, 1f));
		float3 xyz = localToWorld.c0.xyz;
		float3 xyz2 = localToWorld.c1.xyz;
		float3 xyz3 = localToWorld.c2.xyz;
		float3 @float = new float3(math.length(xyz), math.length(xyz2), math.length(xyz3));
		float num = math.max(@float.x, math.max(@float.y, @float.z));
		forceVolumeParameters0.w = volumeParameters0.w * num;
		aabbMin = forceVolumeParameters0.xyz - forceVolumeParameters0.w;
		aabbMax = forceVolumeParameters0.xyz + forceVolumeParameters0.w;
	}

	private void UpdateCylinder(ref float4 volumeParameters0, ref float4x4 localToWorld, ref float3 aabbMin, ref float3 aabbMax, ref float4 forceVolumeParameters0, ref float4 forceVolumeParameters1)
	{
		float3 xyz = localToWorld.c0.xyz;
		float3 xyz2 = localToWorld.c1.xyz;
		float3 xyz3 = localToWorld.c2.xyz;
		float3 @float = new float3(math.length(xyz), math.length(xyz2), math.length(xyz3));
		float y = @float.y;
		float num = math.max(@float.x, @float.z);
		float3 float2 = math.normalize(xyz2);
		float3 float3 = math.normalize(xyz3);
		float3 float4 = math.normalize(xyz);
		float3 xyz4 = localToWorld[3].xyz;
		float num2 = volumeParameters0.x * num;
		float num3 = volumeParameters0.x * num;
		float num4 = volumeParameters0.y * 0.5f;
		float3 float5 = xyz4 - float2 * y * num4;
		float3 float6 = xyz4 + float2 * y * num4;
		forceVolumeParameters0.xyz = float5;
		forceVolumeParameters1.xyz = float6;
		forceVolumeParameters0.w = num2;
		forceVolumeParameters1.w = num3;
		float3 x = float5 - float3 * num2 - float4 * num2;
		float3 x2 = float5 + float3 * num2 + float4 * num2;
		float3 x3 = float5 - float3 * num2 + float4 * num2;
		float3 x4 = float5 + float3 * num2 - float4 * num2;
		float3 x5 = float6 - float3 * num3 - float4 * num3;
		float3 x6 = float6 + float3 * num3 + float4 * num3;
		float3 x7 = float6 - float3 * num3 + float4 * num3;
		float3 y2 = float6 + float3 * num3 - float4 * num3;
		aabbMin = math.min(x, math.min(x2, math.min(x3, math.min(x4, math.min(x5, math.min(x6, math.min(x7, y2)))))));
		aabbMax = math.max(x, math.max(x2, math.max(x3, math.max(x4, math.max(x5, math.max(x6, math.max(x7, y2)))))));
	}

	private void UpdateCone(ref float4 volumeParameters0, ref float4x4 localToWorld, ref float3 aabbMin, ref float3 aabbMax, ref float4 forceVolumeParameters0, ref float4 forceVolumeParameters1)
	{
		float3 xyz = localToWorld.c0.xyz;
		float3 xyz2 = localToWorld.c1.xyz;
		float3 xyz3 = localToWorld.c2.xyz;
		float3 @float = new float3(math.length(xyz), math.length(xyz2), math.length(xyz3));
		float num = math.max(@float.x, math.max(@float.y, @float.z));
		float num2 = volumeParameters0.x * num;
		float y = volumeParameters0.y;
		float num3 = num2 * Mathf.Sin(y * (MathF.PI / 180f) * 0.5f);
		float num4 = Mathf.Cos(MathF.PI / 180f * y * 0.5f) * num2;
		float3 float2 = math.normalize(xyz);
		float3 float3 = math.normalize(xyz2);
		float3 float4 = math.normalize(xyz3);
		float3 xyz4 = localToWorld.c3.xyz;
		float3 float5 = float4 * num4;
		forceVolumeParameters0.xyz = xyz4;
		forceVolumeParameters0.w = num2;
		forceVolumeParameters1.xyz = float4;
		forceVolumeParameters1.w = math.cos(math.radians(y) * 0.5f);
		float3 x = xyz4;
		float3 x2 = xyz4 + float4 * num2;
		float3 x3 = xyz4 + float5 + float3 * num3;
		float3 x4 = xyz4 + float5 - float3 * num3;
		float3 x5 = xyz4 + float5 + float2 * num3;
		float3 y2 = xyz4 + float5 - float2 * num3;
		aabbMin = math.min(x, math.min(x2, math.min(x3, math.min(x4, math.min(x5, y2)))));
		aabbMax = math.max(x, math.max(x2, math.max(x3, math.max(x4, math.max(x5, y2)))));
	}
}
