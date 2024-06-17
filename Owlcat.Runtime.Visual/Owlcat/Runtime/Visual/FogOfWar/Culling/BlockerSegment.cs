using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.FogOfWar.Culling;

[BurstCompile]
public struct BlockerSegment
{
	public float2 PointA;

	public float2 PointB;

	public float HeightMin;

	public float HeightMax;
}
