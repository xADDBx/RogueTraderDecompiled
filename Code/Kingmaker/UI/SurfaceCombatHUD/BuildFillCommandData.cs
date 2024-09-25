using Unity.Burst;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct BuildFillCommandData
{
	public float3 meshOffset;
}
