using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
public struct CornerHeights
{
	public float ne;

	public float nw;

	public float sw;

	public float se;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe float GetCornerHeight(in CornerDirection corner)
	{
		fixed (float* ptr = &ne)
		{
			return ptr[corner.index];
		}
	}
}
