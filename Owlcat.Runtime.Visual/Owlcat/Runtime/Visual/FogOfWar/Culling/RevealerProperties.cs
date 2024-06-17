using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.FogOfWar.Culling;

[BurstCompile]
public struct RevealerProperties
{
	public float2 Center;

	public float Radius;

	public float HeightMin;

	public float HeightMax;
}
