using Unity.Burst;
using Unity.Mathematics;

namespace Kingmaker.Controllers.FogOfWar.Culling;

[BurstCompile]
public struct TargetProperties
{
	public float2 Center;

	public float Radius;
}
