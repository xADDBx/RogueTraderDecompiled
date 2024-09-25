using System.Runtime.InteropServices;
using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[StructLayout(LayoutKind.Explicit)]
[BurstCompile]
public struct CellUnion
{
	[FieldOffset(0)]
	public IntermediateCell intermediateCell;

	[FieldOffset(0)]
	public Cell cell;
}
