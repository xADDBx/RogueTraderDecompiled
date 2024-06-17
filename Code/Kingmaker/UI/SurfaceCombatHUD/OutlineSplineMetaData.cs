using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal readonly struct OutlineSplineMetaData
{
	public readonly bool outer;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public OutlineSplineMetaData(bool outer)
	{
		this.outer = outer;
	}
}
