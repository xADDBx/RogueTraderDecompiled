using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

public struct ResolveCellsJob
{
	[BurstCompile]
	private struct Job : IJob
	{
		private static readonly int2 CoordsOffsetsE = new int2(1, 0);

		private static readonly int2 CoordsOffsetsNE = new int2(1, 1);

		private static readonly int2 CoordsOffsetsN = new int2(0, 1);

		private static readonly int2 CoordsOffsetsNW = new int2(-1, 1);

		private static readonly int2 CoordsOffsetsW = new int2(-1, 0);

		private static readonly int2 CoordsOffsetsSW = new int2(-1, -1);

		private static readonly int2 CoordsOffsetsS = new int2(0, -1);

		private static readonly int2 CoordsOffsetsSE = new int2(1, -1);

		public NativeArray<CellUnion> cells;

		public NativeHashMap<uint, uint> coordsHashToIndexMap;

		public int gridDimensionX;

		public float2 cellSize;

		public float2 halfCellSize;

		public float2 positionOffset;

		public void Execute()
		{
			BuildCoordsHashToIndexMap();
			ResolveCells();
			UpdateReverseCutFlags();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void BuildCoordsHashToIndexMap()
		{
			int i = 0;
			for (int length = cells.Length; i < length; i++)
			{
				IntermediateCell intermediateCell = cells[i].intermediateCell;
				if (intermediateCell.indexInGrid >= 0)
				{
					uint key = math.hash(GetCellCoordinates(intermediateCell.indexInGrid));
					coordsHashToIndexMap[key] = (uint)i;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ResolveCells()
		{
			int i = 0;
			for (int length = cells.Length; i < length; i++)
			{
				IntermediateCell intermediateCell = cells[i].intermediateCell;
				CellUnion value = default(CellUnion);
				if (intermediateCell.indexInGrid < 0)
				{
					value.cell = default(Cell);
				}
				else
				{
					int2 cellCoords = GetCellCoordinates(intermediateCell.indexInGrid);
					PopulateSpatialData(in intermediateCell, ref value.cell, cellCoords);
					PopulateCutData(in intermediateCell, ref value.cell);
					PopulateAdjacencyData(in intermediateCell, ref value.cell, in cellCoords);
				}
				cells[i] = value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PopulateCutData(in IntermediateCell intermediateCell, ref Cell cell)
		{
			if ((intermediateCell.flags & IntermediateCellFlags.ConnectionE) == 0)
			{
				cell.flags |= CellFlags.CutE;
			}
			if ((intermediateCell.flags & IntermediateCellFlags.ConnectionN) == 0)
			{
				cell.flags |= CellFlags.CutN;
			}
			if ((intermediateCell.flags & IntermediateCellFlags.ConnectionW) == 0)
			{
				cell.flags |= CellFlags.CutW;
			}
			if ((intermediateCell.flags & IntermediateCellFlags.ConnectionS) == 0)
			{
				cell.flags |= CellFlags.CutS;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void UpdateReverseCutFlags()
		{
			int i = 0;
			for (int length = cells.Length; i < length; i++)
			{
				Cell cell = cells[i].cell;
				if ((cell.flags & CellFlags.CutE) != 0 && cell.adjacentCellIndices.e != ushort.MaxValue)
				{
					CellUnion value = cells[cell.adjacentCellIndices.e];
					value.cell.flags |= CellFlags.CutW;
					cells[cell.adjacentCellIndices.e] = value;
				}
				if ((cell.flags & CellFlags.CutN) != 0 && cell.adjacentCellIndices.n != ushort.MaxValue)
				{
					CellUnion value2 = cells[cell.adjacentCellIndices.n];
					value2.cell.flags |= CellFlags.CutS;
					cells[cell.adjacentCellIndices.n] = value2;
				}
				if ((cell.flags & CellFlags.CutW) != 0 && cell.adjacentCellIndices.w != ushort.MaxValue)
				{
					CellUnion value3 = cells[cell.adjacentCellIndices.w];
					value3.cell.flags |= CellFlags.CutE;
					cells[cell.adjacentCellIndices.w] = value3;
				}
				if ((cell.flags & CellFlags.CutS) != 0 && cell.adjacentCellIndices.s != ushort.MaxValue)
				{
					CellUnion value4 = cells[cell.adjacentCellIndices.s];
					value4.cell.flags |= CellFlags.CutN;
					cells[cell.adjacentCellIndices.s] = value4;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PopulateSpatialData(in IntermediateCell intermediateCell, ref Cell cell, int2 cellCoords)
		{
			float num = (float)intermediateCell.packedHeight * 0.001f;
			float2 cellRectMin = cellCoords * cellSize + positionOffset;
			float2 cellRectMax = cellRectMin + cellSize;
			float2 @float = cellRectMin + halfCellSize;
			cell.coords = cellCoords;
			cell.center = new float3(@float.x, num, @float.y);
			intermediateCell.packedCornerOffsets.Unpack(num, out cell.cornerHeights.sw, out cell.cornerHeights.se, out cell.cornerHeights.nw, out cell.cornerHeights.ne);
			if (GetQuadSplitSWNE(in cellRectMin, in cellRectMax, in cell.cornerHeights))
			{
				cell.flags |= CellFlags.QuadSplitSWNE;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PopulateAdjacencyData(in IntermediateCell intermediateCell, ref Cell cell, in int2 cellCoords)
		{
			cell.adjacentCellIndices.e = (coordsHashToIndexMap.TryGetValue(math.hash(cellCoords + CoordsOffsetsE), out var item) ? ((ushort)item) : ushort.MaxValue);
			cell.adjacentCellIndices.ne = (coordsHashToIndexMap.TryGetValue(math.hash(cellCoords + CoordsOffsetsNE), out item) ? ((ushort)item) : ushort.MaxValue);
			cell.adjacentCellIndices.n = (coordsHashToIndexMap.TryGetValue(math.hash(cellCoords + CoordsOffsetsN), out item) ? ((ushort)item) : ushort.MaxValue);
			cell.adjacentCellIndices.nw = (coordsHashToIndexMap.TryGetValue(math.hash(cellCoords + CoordsOffsetsNW), out item) ? ((ushort)item) : ushort.MaxValue);
			cell.adjacentCellIndices.w = (coordsHashToIndexMap.TryGetValue(math.hash(cellCoords + CoordsOffsetsW), out item) ? ((ushort)item) : ushort.MaxValue);
			cell.adjacentCellIndices.sw = (coordsHashToIndexMap.TryGetValue(math.hash(cellCoords + CoordsOffsetsSW), out item) ? ((ushort)item) : ushort.MaxValue);
			cell.adjacentCellIndices.s = (coordsHashToIndexMap.TryGetValue(math.hash(cellCoords + CoordsOffsetsS), out item) ? ((ushort)item) : ushort.MaxValue);
			cell.adjacentCellIndices.se = (coordsHashToIndexMap.TryGetValue(math.hash(cellCoords + CoordsOffsetsSE), out item) ? ((ushort)item) : ushort.MaxValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool GetQuadSplitSWNE(in float2 cellRectMin, in float2 cellRectMax, in CornerHeights cornerHeights)
		{
			float3 @float = new float3(cellRectMin.x, cornerHeights.sw, cellRectMin.y);
			float3 float2 = new float3(cellRectMax.x, cornerHeights.se, cellRectMin.y);
			float3 float3 = new float3(cellRectMin.x, cornerHeights.nw, cellRectMax.y);
			float3 float4 = new float3(cellRectMax.x, cornerHeights.ne, cellRectMax.y);
			float3 float5 = math.normalize(float3 - @float);
			float3 float6 = math.normalize(float4 - float3);
			float3 float7 = math.normalize(float2 - float4);
			float3 float8 = math.normalize(@float - float2);
			float3 float9 = math.cross(float8, float5);
			float3 float10 = math.cross(float7, float8);
			float3 float11 = math.cross(float5, float6);
			float3 float12 = math.cross(float6, float7);
			float num = math.max(float9.y, float12.y);
			return math.max(float10.y, float11.y) > num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int2 GetCellCoordinates(int cellIndexInGrid)
		{
			return new int2(cellIndexInGrid % gridDimensionX, cellIndexInGrid / gridDimensionX);
		}
	}

	private readonly NativeArray<CellUnion> m_InOutCells;

	private readonly GridSettings m_GridSettings;

	public ResolveCellsJob(NativeArray<CellUnion> inOutCells, GridSettings gridSettings)
	{
		m_InOutCells = inOutCells;
		m_GridSettings = gridSettings;
	}

	public JobHandle Schedule(JobHandle dependsOn = default(JobHandle))
	{
		NativeHashMap<uint, uint> coordsHashToIndexMap = new NativeHashMap<uint, uint>(m_InOutCells.Length, Allocator.TempJob);
		Job jobData = default(Job);
		jobData.cells = m_InOutCells;
		jobData.coordsHashToIndexMap = coordsHashToIndexMap;
		jobData.gridDimensionX = m_GridSettings.gridDimensionX;
		jobData.cellSize = new float2(m_GridSettings.cellSize);
		jobData.halfCellSize = new float2(m_GridSettings.cellSize / 2f);
		jobData.positionOffset = m_GridSettings.positionOffset;
		JobHandle dependsOn2 = dependsOn;
		dependsOn2 = jobData.Schedule(dependsOn2);
		return coordsHashToIndexMap.Dispose(dependsOn2);
	}
}
