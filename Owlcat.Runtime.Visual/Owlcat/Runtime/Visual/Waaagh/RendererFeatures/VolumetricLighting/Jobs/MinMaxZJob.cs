using Owlcat.Runtime.Visual.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs;

[BurstCompile]
public struct MinMaxZJob : IJobParallelFor
{
	[ReadOnly]
	public float4x4 WorldToViewMatrix;

	public NativeArray<LocalFogDescriptor> LocalFogDescs;

	public void Execute(int index)
	{
		LocalFogDescriptor value = LocalFogDescs[index];
		if (value.IsVisible)
		{
			OrientedBBox orientedBBox = new OrientedBBox(math.mul(WorldToViewMatrix, float4x4.TRS(value.Position, value.Rotation, value.Size)));
			float3 @float = orientedBBox.center - orientedBBox.right * orientedBBox.extentX - orientedBBox.up * orientedBBox.extentY - orientedBBox.forward * orientedBBox.extentZ;
			float3 float2 = orientedBBox.center - orientedBBox.right * orientedBBox.extentX + orientedBBox.up * orientedBBox.extentY - orientedBBox.forward * orientedBBox.extentZ;
			float3 float3 = orientedBBox.center - orientedBBox.right * orientedBBox.extentX - orientedBBox.up * orientedBBox.extentY + orientedBBox.forward * orientedBBox.extentZ;
			float3 float4 = orientedBBox.center - orientedBBox.right * orientedBBox.extentX + orientedBBox.up * orientedBBox.extentY + orientedBBox.forward * orientedBBox.extentZ;
			float3 float5 = orientedBBox.center + orientedBBox.right * orientedBBox.extentX - orientedBBox.up * orientedBBox.extentY - orientedBBox.forward * orientedBBox.extentZ;
			float3 float6 = orientedBBox.center + orientedBBox.right * orientedBBox.extentX + orientedBBox.up * orientedBBox.extentY - orientedBBox.forward * orientedBBox.extentZ;
			float3 float7 = orientedBBox.center + orientedBBox.right * orientedBBox.extentX - orientedBBox.up * orientedBBox.extentY + orientedBBox.forward * orientedBBox.extentZ;
			float3 float8 = orientedBBox.center + orientedBBox.right * orientedBBox.extentX + orientedBBox.up * orientedBBox.extentY + orientedBBox.forward * orientedBBox.extentZ;
			value.MinZ = math.min(@float.z, float2.z);
			value.MinZ = math.min(value.MinZ, float3.z);
			value.MinZ = math.min(value.MinZ, float4.z);
			value.MinZ = math.min(value.MinZ, float5.z);
			value.MinZ = math.min(value.MinZ, float6.z);
			value.MinZ = math.min(value.MinZ, float7.z);
			value.MinZ = math.min(value.MinZ, float8.z);
			value.MaxZ = math.max(@float.z, float2.z);
			value.MaxZ = math.max(value.MaxZ, float3.z);
			value.MaxZ = math.max(value.MaxZ, float4.z);
			value.MaxZ = math.max(value.MaxZ, float5.z);
			value.MaxZ = math.max(value.MaxZ, float6.z);
			value.MaxZ = math.max(value.MaxZ, float7.z);
			value.MaxZ = math.max(value.MaxZ, float8.z);
			value.MeanZ = (value.MinZ + value.MaxZ) / 2f;
		}
		else
		{
			value.MinZ = float.MaxValue;
			value.MinZ = float.MaxValue;
			value.MeanZ = float.MaxValue;
		}
		LocalFogDescs[index] = value;
	}
}
