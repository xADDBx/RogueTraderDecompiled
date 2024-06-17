using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[BurstCompile]
public struct CameraInfo
{
	public float4x4 viewMatrix;

	public float4x4 viewMatrixInverse;

	public float4x4 viewProjectionMatrix;
}
