using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct OutlinePlotter
{
	private readonly float m_HalfCellSize;

	private readonly float m_TurnSmoothDistance;

	private readonly OutlineCellFilter m_ShapeMask;

	private readonly CellBuffer m_CellBuffer;

	private EdgeBuffer m_EdgeBuffer;

	private SplinePlotter<OutlineSplineMetaData, OutlineSplineMeshBuilder> m_SplinePlotter;

	private OutlinePlotterBasis m_Basis;

	private float3 m_CursorPosition;

	public OutlinePlotter(OutlineCellFilter shapeMask, CellBuffer cellBuffer, EdgeBuffer edgeBuffer, SplinePlotter<OutlineSplineMetaData, OutlineSplineMeshBuilder> splinePlotter, float halfCellSize, float turnSmoothDistance)
	{
		m_HalfCellSize = halfCellSize;
		m_TurnSmoothDistance = turnSmoothDistance;
		m_EdgeBuffer = edgeBuffer;
		m_ShapeMask = shapeMask;
		m_CellBuffer = cellBuffer;
		m_SplinePlotter = splinePlotter;
		m_Basis = OutlinePlotterBasis.Default;
		m_CursorPosition = default(float3);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Plot(in float3 startPosition, in NativeArray<OutlinePlotCommand> commands)
	{
		m_SplinePlotter.StartLine(startPosition, quaternion.LookRotation(m_Basis.Forward, new float3(0f, 1f, 0f)));
		m_CursorPosition = startPosition;
		foreach (OutlinePlotCommand command2 in commands)
		{
			OutlinePlotCommand command = command2;
			switch (command.code)
			{
			case OutlinePlotCommandCode.Forward:
				PlotForward(in command);
				break;
			case OutlinePlotCommandCode.TurnLeft:
				PlotTurnLeft(in command);
				break;
			case OutlinePlotCommandCode.TurnRight:
				PlotTurnRight(in command);
				break;
			case OutlinePlotCommandCode.TurnBackward:
				PlotTurnBackward(in command);
				break;
			}
		}
		m_SplinePlotter.FinishLine(new OutlineSplineMetaData(outer: false));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PlotForward(in OutlinePlotCommand command)
	{
		int firstSegmentCellIndex = command.firstSegmentCellIndex;
		CellEdgeSegment edgeSegment = m_Basis.EastSideNorthPartEdge;
		bool masked = !TestAndWriteEdgeSegment(firstSegmentCellIndex, in edgeSegment);
		int secondSegmentCellIndex = command.secondSegmentCellIndex;
		edgeSegment = m_Basis.EastSideSouthPartEdge;
		bool masked2 = !TestAndWriteEdgeSegment(secondSegmentCellIndex, in edgeSegment);
		Cell cell = m_CellBuffer.GetCell(command.firstSegmentCellIndex);
		Cell cell2 = m_CellBuffer.GetCell(command.secondSegmentCellIndex);
		CornerDirection corner = m_Basis.CornerNE;
		float cornerHeight = cell.GetCornerHeight(in corner);
		corner = m_Basis.CornerNE;
		float cornerHeight2 = cell.GetCornerHeight(in corner);
		CornerDirection corner2 = m_Basis.CornerNE;
		float y = math.lerp(cornerHeight2, cell2.GetCornerHeight(in corner2), 0.5f);
		float3 @float = m_Basis.Forward * m_HalfCellSize;
		float3 float2 = m_CursorPosition + @float;
		float2.y = cornerHeight;
		float3 float3 = float2 + @float;
		float3.y = y;
		m_SplinePlotter.PushPoint(float2, 0f, 0f, masked);
		m_SplinePlotter.PushPoint(float3, 0f, 0f, masked2);
		m_CursorPosition = float3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PlotTurnRight(in OutlinePlotCommand command)
	{
		int firstSegmentCellIndex = command.firstSegmentCellIndex;
		CellEdgeSegment edgeSegment = m_Basis.EastSideNorthPartEdge;
		bool masked = !TestAndWriteEdgeSegment(firstSegmentCellIndex, in edgeSegment);
		int secondSegmentCellIndex = command.secondSegmentCellIndex;
		edgeSegment = m_Basis.SouthSideWestPartEdge;
		bool masked2 = !TestAndWriteEdgeSegment(secondSegmentCellIndex, in edgeSegment);
		Cell cell = m_CellBuffer.GetCell(command.firstSegmentCellIndex);
		Cell cell2 = m_CellBuffer.GetCell(command.secondSegmentCellIndex);
		CornerDirection corner = m_Basis.CornerNE;
		float cornerHeight = cell.GetCornerHeight(in corner);
		corner = m_Basis.CornerNE;
		float cornerHeight2 = cell.GetCornerHeight(in corner);
		CornerDirection corner2 = m_Basis.CornerSE;
		float y = math.lerp(cornerHeight2, cell2.GetCornerHeight(in corner2), 0.5f);
		float3 @float = m_Basis.Forward * m_HalfCellSize;
		float3 float2 = m_Basis.Right * m_HalfCellSize;
		float3 float3 = m_CursorPosition + @float;
		float3.y = cornerHeight;
		float3 float4 = float3 + float2;
		float4.y = y;
		m_SplinePlotter.PushPoint(float3, m_TurnSmoothDistance, 0f, masked);
		m_SplinePlotter.PushPoint(float4, 0f, 0f, masked2);
		m_CursorPosition = float4;
		m_Basis.TurnClockwise90();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PlotTurnLeft(in OutlinePlotCommand command)
	{
		int firstSegmentCellIndex = command.firstSegmentCellIndex;
		CellEdgeSegment edgeSegment = m_Basis.EastSideNorthPartEdge;
		bool masked = !TestAndWriteEdgeSegment(firstSegmentCellIndex, in edgeSegment);
		int secondSegmentCellIndex = command.secondSegmentCellIndex;
		edgeSegment = m_Basis.NorthSideEastPartEdge;
		bool masked2 = !TestAndWriteEdgeSegment(secondSegmentCellIndex, in edgeSegment);
		Cell cell = m_CellBuffer.GetCell(command.firstSegmentCellIndex);
		Cell cell2 = m_CellBuffer.GetCell(command.secondSegmentCellIndex);
		CornerDirection corner = m_Basis.CornerNE;
		float cornerHeight = cell.GetCornerHeight(in corner);
		corner = m_Basis.CornerNE;
		float cornerHeight2 = cell.GetCornerHeight(in corner);
		CornerDirection corner2 = m_Basis.CornerNW;
		float y = math.lerp(cornerHeight2, cell2.GetCornerHeight(in corner2), 0.5f);
		float3 @float = m_Basis.Forward * m_HalfCellSize;
		float3 float2 = m_Basis.Left * m_HalfCellSize;
		float3 float3 = m_CursorPosition + @float;
		float3.y = cornerHeight;
		float3 float4 = float3 + float2;
		float4.y = y;
		m_SplinePlotter.PushPoint(float3, m_TurnSmoothDistance, 0f, masked);
		m_SplinePlotter.PushPoint(float4, 0f, 0f, masked2);
		m_CursorPosition = float4;
		m_Basis.TurnCounterClockwise90();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PlotTurnBackward(in OutlinePlotCommand command)
	{
		int firstSegmentCellIndex = command.firstSegmentCellIndex;
		CellEdgeSegment edgeSegment = m_Basis.EastSideNorthPartEdge;
		bool masked = !TestAndWriteEdgeSegment(firstSegmentCellIndex, in edgeSegment);
		int secondSegmentCellIndex = command.secondSegmentCellIndex;
		edgeSegment = m_Basis.WestSideNorthPartEdge;
		bool masked2 = !TestAndWriteEdgeSegment(secondSegmentCellIndex, in edgeSegment);
		Cell cell = m_CellBuffer.GetCell(command.firstSegmentCellIndex);
		Cell cell2 = m_CellBuffer.GetCell(command.secondSegmentCellIndex);
		CornerDirection corner = m_Basis.CornerNE;
		float cornerHeight = cell.GetCornerHeight(in corner);
		corner = m_Basis.CornerNE;
		float cornerHeight2 = cell.GetCornerHeight(in corner);
		CornerDirection corner2 = m_Basis.CornerSW;
		float y = math.lerp(cornerHeight2, cell2.GetCornerHeight(in corner2), 0.5f);
		float3 @float = m_Basis.Forward * m_HalfCellSize;
		float3 float2 = m_CursorPosition + @float;
		float2.y = cornerHeight;
		float3 cursorPosition = m_CursorPosition;
		cursorPosition.y = y;
		float num = math.sign(cursorPosition.y - m_CursorPosition.y);
		float3 float3 = float2 + math.normalize(m_CursorPosition - float2) * m_TurnSmoothDistance;
		float3 float4 = float2 + math.normalize(cursorPosition - float2) * m_TurnSmoothDistance;
		float3 float5 = math.lerp(float3, float4, 0.5f);
		float3 forward = math.normalize(float4 - float3);
		float3 float6 = num * math.normalize(float5 - float2);
		quaternion rotation = quaternion.LookRotation(forward, float6);
		quaternion rotation2 = quaternion.LookRotation(forward, -float6);
		float smoothDistance = math.distance(float3, float4) / 2f;
		m_SplinePlotter.PushPoint(float3, smoothDistance, 0f, masked);
		m_SplinePlotter.PushPoint(float5, rotation, 0f, 0f, masked);
		m_SplinePlotter.BreakLine();
		m_SplinePlotter.PushPoint(float5, rotation2, 0f, 0f, masked2);
		m_SplinePlotter.PushPoint(float4, rotation2, smoothDistance, 0f, masked2);
		m_SplinePlotter.PushPoint(cursorPosition, 0f, 0f, masked2);
		m_CursorPosition = cursorPosition;
		m_Basis.Turn180();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TestAndWriteEdgeSegment(int cellIndex, in CellEdgeSegment edgeSegment)
	{
		if (m_ShapeMask.Test(cellIndex) && m_EdgeBuffer.Test(cellIndex, edgeSegment))
		{
			m_EdgeBuffer.Write(cellIndex, edgeSegment);
			return true;
		}
		return false;
	}
}
