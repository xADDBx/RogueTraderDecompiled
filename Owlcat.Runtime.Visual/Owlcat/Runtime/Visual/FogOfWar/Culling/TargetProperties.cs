using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.FogOfWar.Culling;

[BurstCompile]
public struct TargetProperties
{
	public float2 Center;

	public float Radius;
}
