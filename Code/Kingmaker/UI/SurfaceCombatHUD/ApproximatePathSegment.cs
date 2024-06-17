using Unity.Burst;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct ApproximatePathSegment
{
	public float3 direction;

	public float length;

	public float spatialDistanceAtEnd;
}
