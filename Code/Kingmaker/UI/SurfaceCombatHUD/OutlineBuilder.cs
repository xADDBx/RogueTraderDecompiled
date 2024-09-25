using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct OutlineBuilder
{
	private readonly CellBuffer m_CellBuffer;

	private readonly NativeArray<ushort> m_ChunkAreaMasks;

	private readonly float m_HalfCellSize;

	private readonly float m_TurnSmoothDistance;

	private NativeArray<BezierPoint> m_BezierPoints;

	private NativeArray<byte> m_ProcessStatusFlags;

	private NativeList<OutlinePlotCommand> m_OutlinePlotCommands;

	private byte m_IterationNumber;

	public OutlineBuilder(CellBuffer cellBuffer, NativeArray<ushort> chunkAreaMasks, float halfCellSize, float turnSmoothDistance, NativeArray<BezierPoint> bezierPoints, NativeArray<byte> processStatusFlags, NativeList<OutlinePlotCommand> outlinePlotCommands)
	{
		m_CellBuffer = cellBuffer;
		m_ChunkAreaMasks = chunkAreaMasks;
		m_HalfCellSize = halfCellSize;
		m_TurnSmoothDistance = turnSmoothDistance;
		m_BezierPoints = bezierPoints;
		m_ProcessStatusFlags = processStatusFlags;
		m_OutlinePlotCommands = outlinePlotCommands;
		m_IterationNumber = 0;
	}

	public void Execute(OutlineType outlineType, in OutlineCellFilter shape, in OutlineCellFilter shapeMask, ref EdgeBuffer edgeBuffer, ref OutlineSplineMeshBuilder splineMeshBuilder)
	{
		m_IterationNumber++;
		SplinePlotter<OutlineSplineMetaData, OutlineSplineMeshBuilder> splinePlotter = new SplinePlotter<OutlineSplineMetaData, OutlineSplineMeshBuilder>(splineMeshBuilder, m_BezierPoints, 0f);
		EdgeBuffer edgeBuffer2 = edgeBuffer;
		SplinePlotter<OutlineSplineMetaData, OutlineSplineMeshBuilder> splinePlotter2 = splinePlotter;
		float halfCellSize = m_HalfCellSize;
		float turnSmoothDistance = m_TurnSmoothDistance;
		OutlinePlotter outlinePlotter = new OutlinePlotter(shapeMask, m_CellBuffer, edgeBuffer2, splinePlotter2, halfCellSize, turnSmoothDistance);
		OutlineTracer2 outlineTracer = new OutlineTracer2(m_HalfCellSize * 2f, m_IterationNumber, m_CellBuffer, m_ChunkAreaMasks, m_ProcessStatusFlags, m_OutlinePlotCommands, shape);
		OutlineTraceResult result;
		while (outlineTracer.Trace(out result))
		{
			if (outlineType switch
			{
				OutlineType.Default => true, 
				OutlineType.Inner => !result.outer, 
				OutlineType.Outer => result.outer, 
				_ => false, 
			})
			{
				ref readonly float3 startPosition = ref result.startPosition;
				NativeArray<OutlinePlotCommand> commands = m_OutlinePlotCommands;
				outlinePlotter.Plot(in startPosition, in commands);
			}
			m_OutlinePlotCommands.Clear();
		}
	}
}
