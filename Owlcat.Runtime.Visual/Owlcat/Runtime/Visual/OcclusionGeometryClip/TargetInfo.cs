using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[BurstCompile]
public struct TargetInfo
{
	public float3 position;

	public float2 size;

	public float2 dynamicSize;

	public TargetInsideBoxOcclusionMode targetInsideBoxOcclusionMode;
}
