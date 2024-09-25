using Owlcat.Runtime.Visual.Lighting;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs;

[BurstCompile]
public struct LocalFogDescriptor
{
	public float3 Position;

	public quaternion Rotation;

	public float3 Size;

	public LocalVolumetricFogEngineData Data;

	public float MinZ;

	public float MaxZ;

	public float MeanZ;

	public bool IsVisible;
}
