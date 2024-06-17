using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct OutlineTracer2
{
	private struct Basis
	{
		public GridDirection localAdjacencyDirectionE;

		public GridDirection localAdjacencyDirectionW;

		public GridDirection localAdjacencyDirectionN;

		public GridDirection localAdjacencyDirectionS;

		public GridDirection localAdjacencyDirectionNE;

		public EdgeDirection localFenceDirectionW;

		public EdgeDirection localFenceDirectionE;

		public EdgeDirection localFenceDirectionS;

		public EdgeDirection localFenceDirectionN;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Basis MakeDefault()
		{
			Basis result = default(Basis);
			result.localAdjacencyDirectionE = GridDirection.E;
			result.localAdjacencyDirectionW = GridDirection.W;
			result.localAdjacencyDirectionN = GridDirection.N;
			result.localAdjacencyDirectionS = GridDirection.S;
			result.localAdjacencyDirectionNE = GridDirection.NE;
			result.localFenceDirectionW = EdgeDirection.W;
			result.localFenceDirectionE = EdgeDirection.E;
			result.localFenceDirectionS = EdgeDirection.S;
			result.localFenceDirectionN = EdgeDirection.N;
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void TurnCounterClockwise90()
		{
			localAdjacencyDirectionE = localAdjacencyDirectionE.TurnCounterClockwise90();
			localAdjacencyDirectionW = localAdjacencyDirectionW.TurnCounterClockwise90();
			localAdjacencyDirectionN = localAdjacencyDirectionN.TurnCounterClockwise90();
			localAdjacencyDirectionS = localAdjacencyDirectionS.TurnCounterClockwise90();
			localAdjacencyDirectionNE = localAdjacencyDirectionNE.TurnCounterClockwise90();
			localFenceDirectionW = localFenceDirectionW.TurnCounterClockwise90();
			localFenceDirectionE = localFenceDirectionE.TurnCounterClockwise90();
			localFenceDirectionS = localFenceDirectionS.TurnCounterClockwise90();
			localFenceDirectionN = localFenceDirectionN.TurnCounterClockwise90();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void TurnClockwise90()
		{
			localAdjacencyDirectionE = localAdjacencyDirectionE.TurnClockwise90();
			localAdjacencyDirectionW = localAdjacencyDirectionW.TurnClockwise90();
			localAdjacencyDirectionN = localAdjacencyDirectionN.TurnClockwise90();
			localAdjacencyDirectionS = localAdjacencyDirectionS.TurnClockwise90();
			localAdjacencyDirectionNE = localAdjacencyDirectionNE.TurnClockwise90();
			localFenceDirectionW = localFenceDirectionW.TurnClockwise90();
			localFenceDirectionE = localFenceDirectionE.TurnClockwise90();
			localFenceDirectionS = localFenceDirectionS.TurnClockwise90();
			localFenceDirectionN = localFenceDirectionN.TurnClockwise90();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Turn180()
		{
			localAdjacencyDirectionE = localAdjacencyDirectionE.Turn180();
			localAdjacencyDirectionW = localAdjacencyDirectionW.Turn180();
			localAdjacencyDirectionN = localAdjacencyDirectionN.Turn180();
			localAdjacencyDirectionS = localAdjacencyDirectionS.Turn180();
			localAdjacencyDirectionNE = localAdjacencyDirectionNE.Turn180();
			localFenceDirectionW = localFenceDirectionW.Turn180();
			localFenceDirectionE = localFenceDirectionE.Turn180();
			localFenceDirectionS = localFenceDirectionS.Turn180();
			localFenceDirectionN = localFenceDirectionN.Turn180();
		}
	}

	private const int kMaxIterations = 10000;

	private readonly float m_CellSize;

	private readonly byte m_IterationNumber;

	private readonly CellBuffer m_CellBuffer;

	private readonly NativeArray<ushort> m_ChunkAreaMasks;

	private NativeArray<byte> m_ProcessedStatusFlags;

	private NativeList<OutlinePlotCommand> m_Commands;

	private readonly OutlineCellFilter m_ShapeFilter;

	private int m_CellIndex;

	private int m_TurnsCounter;

	public OutlineTracer2(float cellSize, byte iterationNumber, CellBuffer cellBuffer, NativeArray<ushort> chunkAreaMasks, NativeArray<byte> processedStatusFlags, NativeList<OutlinePlotCommand> commands, OutlineCellFilter shapeFilter)
	{
		m_CellSize = cellSize;
		m_IterationNumber = iterationNumber;
		m_CellBuffer = cellBuffer;
		m_ChunkAreaMasks = chunkAreaMasks;
		m_ProcessedStatusFlags = processedStatusFlags;
		m_Commands = commands;
		m_ShapeFilter = shapeFilter;
		m_CellIndex = -1;
		m_TurnsCounter = 0;
	}

	public bool Trace(out OutlineTraceResult result)
	{
		m_CellIndex++;
		while (m_CellIndex < m_CellBuffer.Length)
		{
			int num = m_CellIndex / 64;
			int num2 = (num + 1) * 64;
			if (m_ShapeFilter.TestArea(m_ChunkAreaMasks[num]))
			{
				while (m_CellIndex < num2)
				{
					if (TryTrace(m_CellIndex, out result))
					{
						return true;
					}
					m_CellIndex++;
				}
			}
			else
			{
				m_CellIndex = num2;
			}
		}
		result = default(OutlineTraceResult);
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryTrace(int startCellIndex, out OutlineTraceResult result)
	{
		if (!m_ShapeFilter.Test(startCellIndex))
		{
			result = default(OutlineTraceResult);
			return false;
		}
		if (IsCellProcessed(startCellIndex))
		{
			result = default(OutlineTraceResult);
			return false;
		}
		Cell cell = m_CellBuffer.GetCell(startCellIndex);
		if (cell.TryGetAdjacent(in GridDirection.E, out var cellIndex) && cell.HasNoCut(in EdgeDirection.E) && IsCellBelongsToShape(cellIndex))
		{
			result = default(OutlineTraceResult);
			return false;
		}
		result = Trace(startCellIndex);
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private OutlineTraceResult Trace(int startCellIndex)
	{
		int cellIndex = startCellIndex;
		Cell cell = m_CellBuffer.GetCell(cellIndex);
		Basis basis = Basis.MakeDefault();
		EdgeDirection localFenceDirectionE = basis.localFenceDirectionE;
		CornerDirection corner = new CornerDirection(3);
		CornerDirection corner2 = new CornerDirection(0);
		float3 startPosition = cell.center + new float3(m_CellSize / 2f, 0f, 0f);
		startPosition.y = math.lerp(cell.GetCornerHeight(in corner), cell.GetCornerHeight(in corner2), 0.5f);
		int num = 0;
		m_TurnsCounter = 0;
		do
		{
			if (basis.localFenceDirectionE == EdgeDirection.E)
			{
				MarkCellAsProcessed(cellIndex);
			}
			ProcessLocalEdgeE(ref cellIndex, ref cell, ref basis);
		}
		while (++num <= 10000 && (cellIndex != startCellIndex || basis.localFenceDirectionE != localFenceDirectionE));
		return new OutlineTraceResult(m_TurnsCounter > 0, startPosition);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessLocalEdgeE(ref int cellIndex, ref Cell cell, ref Basis basis)
	{
		ushort cellIndex2;
		bool flag = cell.TryGetAdjacent(in basis.localAdjacencyDirectionE, out cellIndex2) && IsCellBelongsToShape(cellIndex2);
		ushort cellIndex3;
		bool flag2 = cell.TryGetAdjacent(in basis.localAdjacencyDirectionN, out cellIndex3) && IsCellBelongsToShape(cellIndex3);
		ushort cellIndex4;
		bool flag3 = cell.TryGetAdjacent(in basis.localAdjacencyDirectionNE, out cellIndex4) && IsCellBelongsToShape(cellIndex4);
		Cell cell2 = (flag ? m_CellBuffer.GetCell(cellIndex2) : default(Cell));
		Cell cell3 = (flag2 ? m_CellBuffer.GetCell(cellIndex3) : default(Cell));
		Cell cell4 = (flag3 ? m_CellBuffer.GetCell(cellIndex4) : default(Cell));
		bool flag4 = flag && flag3 && flag2 && cell.HasNoCut(in basis.localFenceDirectionN) && cell3.HasNoCut(in basis.localFenceDirectionE) && cell4.HasNoCut(in basis.localFenceDirectionS);
		bool flag5 = flag2 && cell.HasNoCut(in basis.localFenceDirectionN);
		bool flag6 = flag3 && ((cell.HasNoCut(in basis.localFenceDirectionE) && cell4.HasNoCut(in basis.localFenceDirectionS)) || (cell.HasNoCut(in basis.localFenceDirectionN) && cell4.HasNoCut(in basis.localFenceDirectionW)));
		if (!flag4 && flag)
		{
			if (flag6 && cell4.HasNoCut(in basis.localFenceDirectionS))
			{
				flag4 = true;
			}
			if (flag5 && cell3.HasNoCut(in basis.localFenceDirectionE) && cell2.HasNoCut(in basis.localFenceDirectionN))
			{
				flag4 = true;
			}
		}
		if (flag4)
		{
			ref NativeList<OutlinePlotCommand> commands = ref m_Commands;
			OutlinePlotCommand value = new OutlinePlotCommand(OutlinePlotCommandCode.TurnBackward, cellIndex, cellIndex2);
			commands.Add(in value);
			m_TurnsCounter -= 2;
			cellIndex = cellIndex2;
			cell = cell2;
			basis.Turn180();
		}
		else if (flag6)
		{
			ref NativeList<OutlinePlotCommand> commands2 = ref m_Commands;
			OutlinePlotCommand value = new OutlinePlotCommand(OutlinePlotCommandCode.TurnRight, cellIndex, cellIndex4);
			commands2.Add(in value);
			m_TurnsCounter--;
			cellIndex = cellIndex4;
			cell = cell4;
			basis.TurnClockwise90();
		}
		else if (flag5)
		{
			ref NativeList<OutlinePlotCommand> commands3 = ref m_Commands;
			OutlinePlotCommand value = new OutlinePlotCommand(OutlinePlotCommandCode.Forward, cellIndex, cellIndex3);
			commands3.Add(in value);
			cellIndex = cellIndex3;
			cell = cell3;
		}
		else
		{
			ref NativeList<OutlinePlotCommand> commands4 = ref m_Commands;
			OutlinePlotCommand value = new OutlinePlotCommand(OutlinePlotCommandCode.TurnLeft, cellIndex, cellIndex);
			commands4.Add(in value);
			m_TurnsCounter++;
			basis.TurnCounterClockwise90();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsCellBelongsToShape(int cellIndex)
	{
		int index = cellIndex / 64;
		if (m_ShapeFilter.TestArea(m_ChunkAreaMasks[index]))
		{
			return m_ShapeFilter.Test(cellIndex);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsCellProcessed(int index)
	{
		return m_ProcessedStatusFlags[index] == m_IterationNumber;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void MarkCellAsProcessed(int index)
	{
		m_ProcessedStatusFlags[index] = m_IterationNumber;
	}
}
