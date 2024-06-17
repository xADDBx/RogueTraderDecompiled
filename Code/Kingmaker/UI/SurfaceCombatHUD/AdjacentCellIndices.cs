using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
public struct AdjacentCellIndices
{
	public ushort e;

	public ushort ne;

	public ushort n;

	public ushort nw;

	public ushort w;

	public ushort sw;

	public ushort s;

	public ushort se;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool HasAdjacent(in GridDirection direction)
	{
		fixed (ushort* ptr = &e)
		{
			return ptr[direction.m_Index] != ushort.MaxValue;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool TryGetAdjacent(in GridDirection direction, out ushort cellIndex)
	{
		fixed (ushort* ptr = &e)
		{
			cellIndex = ptr[direction.m_Index];
			return cellIndex != ushort.MaxValue;
		}
	}
}
