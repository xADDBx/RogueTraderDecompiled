using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.FogOfWar.Culling;

[BurstCompile]
internal struct BlockerPlaneSet
{
	public float4 PlaneA;

	public float4 PlaneB;

	public float4 PlaneC;
}
