using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
public struct SurfaceCellFilterData
{
	public int belongToAllAreaMask;

	public int belongToAnyAreasMask;

	public int notBelongToAnyAreasMask;
}
