using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
public struct Cell
{
	public int2 coords;

	public float3 center;

	public AdjacentCellIndices adjacentCellIndices;

	public CornerHeights cornerHeights;

	public CellFlags flags;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetCornerHeight(in CornerDirection corner)
	{
		return cornerHeights.GetCornerHeight(in corner);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool HasAdjacent(in GridDirection direction)
	{
		return adjacentCellIndices.HasAdjacent(in direction);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public bool TryGetAdjacent(in GridDirection direction, out ushort cellIndex)
	{
		return adjacentCellIndices.TryGetAdjacent(in direction, out cellIndex);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool HasCut(in EdgeDirection direction)
	{
		return ((uint)flags & (uint)direction.Flag) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool HasNoCut(in EdgeDirection direction)
	{
		return ((uint)flags & (uint)direction.Flag) == 0;
	}
}
