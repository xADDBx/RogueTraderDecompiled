using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct EdgeBuffer
{
	private readonly CellBuffer m_CellBuffer;

	private readonly bool m_TestDisabled;

	private NativeArray<byte> m_EdgeSegmentFlagArray;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EdgeBuffer(CellBuffer cellBuffer, NativeArray<byte> edgeSegmentFlagArray, bool testDisabled)
	{
		m_CellBuffer = cellBuffer;
		m_EdgeSegmentFlagArray = edgeSegmentFlagArray;
		m_TestDisabled = testDisabled;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public bool Test(int cellIndex, CellEdgeSegment edgeSegment)
	{
		if (m_TestDisabled)
		{
			return true;
		}
		if ((m_EdgeSegmentFlagArray[cellIndex] & edgeSegment.Flag) != 0)
		{
			return false;
		}
		Cell cell = m_CellBuffer.GetCell(cellIndex);
		GridDirection direction = edgeSegment.GetAdjacentCellDirection();
		if (cell.TryGetAdjacent(in direction, out var cellIndex2) && (m_EdgeSegmentFlagArray[cellIndex2] & edgeSegment.MirrorSide().Flag) != 0)
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Write(int cellIndex, CellEdgeSegment edgeSegment)
	{
		m_EdgeSegmentFlagArray[cellIndex] |= edgeSegment.Flag;
	}
}
