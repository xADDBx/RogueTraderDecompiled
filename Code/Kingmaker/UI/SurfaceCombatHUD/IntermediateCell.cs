using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
public struct IntermediateCell
{
	public int indexInGrid;

	public int packedHeight;

	public PackedCornerOffsets packedCornerOffsets;

	public IntermediateCellFlags flags;
}
