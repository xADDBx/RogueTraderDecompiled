using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct AppendMeshCommandData
{
	public int materialId;
}
