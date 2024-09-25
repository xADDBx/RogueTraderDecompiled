using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct WriteFillCommandData
{
	public int materialId;

	public int shapeId;

	public SurfaceCellFilterData selectFilter;
}
