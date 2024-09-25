using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs;

[BurstCompile]
public struct ExtractLocalFogDataJob : IJobParallelFor
{
	[ReadOnly]
	public float4x4 WorldToViewMatrix;

	[ReadOnly]
	public NativeArray<LocalFogDescriptor> LocalFogDescriptors;

	[WriteOnly]
	public NativeArray<LocalVolumetricFogBounds> Obbs;

	[WriteOnly]
	public NativeArray<LocalVolumetricFogEngineData> LocalFogEngineData;

	public void Execute(int index)
	{
		LocalFogDescriptor localFogDescriptor = LocalFogDescriptors[index];
		OrientedBBox orientedBBox = new OrientedBBox(math.mul(WorldToViewMatrix, float4x4.TRS(localFogDescriptor.Position, localFogDescriptor.Rotation, localFogDescriptor.Size)));
		Obbs[index] = new LocalVolumetricFogBounds
		{
			center = orientedBBox.center,
			extentX = orientedBBox.extentX,
			extentY = orientedBBox.extentY,
			extentZ = orientedBBox.extentZ,
			right = orientedBBox.right,
			up = orientedBBox.up,
			minZ = localFogDescriptor.MinZ,
			maxZ = localFogDescriptor.MaxZ
		};
		LocalFogEngineData[index] = localFogDescriptor.Data;
	}
}
