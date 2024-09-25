using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct ShadowFaceData
{
	public float4 CullingSphere;

	public float4 FaceDirection;

	public float4x4 WorldToShadow;
}
