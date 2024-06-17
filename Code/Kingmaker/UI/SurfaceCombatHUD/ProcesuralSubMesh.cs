using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct ProcesuralSubMesh
{
	public byte materialId;

	public int indexStart;

	public int indexCount;
}
