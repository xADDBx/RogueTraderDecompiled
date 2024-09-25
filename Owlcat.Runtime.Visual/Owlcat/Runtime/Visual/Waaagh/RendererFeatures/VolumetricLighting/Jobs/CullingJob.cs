using Owlcat.Runtime.Visual.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs;

[BurstCompile]
public struct CullingJob : IJob
{
	[ReadOnly]
	public Frustum CameraFrustum;

	[ReadOnly]
	public int TotalVolumesCount;

	public NativeArray<LocalFogDescriptor> FogDescs;

	[WriteOnly]
	public NativeArray<int> VisibleCounter;

	public void Execute()
	{
		int num = 0;
		for (int i = 0; i < TotalVolumesCount; i++)
		{
			LocalFogDescriptor value = FogDescs[i];
			OrientedBBox obb = new OrientedBBox(float4x4.TRS(value.Position, value.Rotation, value.Size));
			value.IsVisible = GeometryUtils.Overlap(obb, CameraFrustum, 6, 8);
			if (value.IsVisible)
			{
				num++;
			}
			FogDescs[i] = value;
		}
		VisibleCounter[0] = num;
		for (int j = TotalVolumesCount; j < 512; j++)
		{
			LocalFogDescriptor value2 = FogDescs[j];
			value2.IsVisible = false;
			value2.MinZ = float.MaxValue;
			value2.MaxZ = float.MaxValue;
			value2.MeanZ = float.MaxValue;
			FogDescs[j] = value2;
		}
	}
}
