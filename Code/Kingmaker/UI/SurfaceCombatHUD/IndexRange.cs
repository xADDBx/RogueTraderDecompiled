using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
public struct IndexRange
{
	public int begin;

	public int end;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IndexRange(int begin, int end)
	{
		this.begin = begin;
		this.end = end;
	}

	public override string ToString()
	{
		return $"{{{begin}:{end}}}";
	}
}
