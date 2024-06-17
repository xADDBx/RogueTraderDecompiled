using Unity.Burst;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct SplinePoint
{
	public float3 position;

	public quaternion rotation;

	public float spatialDistance;

	public float segmentedDistance;

	public bool breakLine;

	public bool masked;

	public bool optional;
}
