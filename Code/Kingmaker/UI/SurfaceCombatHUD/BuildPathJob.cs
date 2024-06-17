using System;
using Kingmaker.Pathfinding;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

internal struct BuildPathJob
{
	private struct ContainerAdapter : IIdentifierContainer, IDisposable
	{
		private NativeHashMap<int, int> m_CellIdToCellIndexMap;

		private readonly CustomGridGraph m_Graph;

		private NativeList<CellUnion> m_Cells;

		private NativeList<int> m_CellIndices;

		public ContainerAdapter(int estimatedCellsCount, CustomGridGraph graph, NativeList<CellUnion> cells, NativeList<int> cellIndices)
		{
			m_Graph = graph;
			m_Cells = cells;
			m_CellIndices = cellIndices;
			m_CellIdToCellIndexMap = new NativeHashMap<int, int>(estimatedCellsCount, Allocator.Temp);
		}

		public void PushRange(int cellIdentifierBegin, int cellIdentifierEnd)
		{
			for (int i = cellIdentifierBegin; i < cellIdentifierEnd; i++)
			{
				Push(i);
			}
		}

		public void Push(int cellIdentifier)
		{
			if (m_CellIdToCellIndexMap.TryGetValue(cellIdentifier, out var item))
			{
				m_CellIndices.Add(in item);
				return;
			}
			CellUnion value = default(CellUnion);
			CustomGridNode customGridNode = m_Graph.nodes[cellIdentifier];
			CustomGridMeshNode customGridMeshNode = m_Graph.meshNodes[cellIdentifier];
			value.intermediateCell.indexInGrid = cellIdentifier;
			value.intermediateCell.packedHeight = customGridNode.position.y;
			if ((customGridNode.flags & (true ? 1u : 0u)) != 0)
			{
				value.intermediateCell.flags = (IntermediateCellFlags)(customGridNode.gridFlags & 0xFu);
			}
			else
			{
				value.intermediateCell.flags = (IntermediateCellFlags)0;
			}
			value.intermediateCell.packedCornerOffsets.packedCornerOffsetNE = customGridMeshNode.packedCornerOffsetNE;
			value.intermediateCell.packedCornerOffsets.packedCornerOffsetNW = customGridMeshNode.packedCornerOffsetNW;
			value.intermediateCell.packedCornerOffsets.packedCornerOffsetSW = customGridMeshNode.packedCornerOffsetSW;
			value.intermediateCell.packedCornerOffsets.packedCornerOffsetSE = customGridMeshNode.packedCornerOffsetSE;
			int value2 = m_Cells.Length;
			m_CellIdToCellIndexMap[cellIdentifier] = value2;
			m_Cells.Add(in value);
			m_CellIndices.Add(in value2);
		}

		public void Dispose()
		{
			m_CellIdToCellIndexMap.Dispose();
		}
	}

	[BurstCompile]
	private struct Job : IJob
	{
		public float m_CellSize;

		public int m_MaterialId;

		public PathLineSettings m_PathLineSettings;

		[ReadOnly]
		public NativeArray<CellUnion> m_Cells;

		[ReadOnly]
		public NativeArray<int> m_CellIndices;

		[ReadOnly]
		public NativeArray<BezierPoint> m_BezierPoints;

		public ProceduralMesh<LineVertex, uint> m_ProceduralMesh;

		public float3 positionOffset;

		public NativeList<ApproximatePathSegment> ApproximatePath;

		public unsafe void Execute()
		{
			MeshComposer<LineVertex, uint>.State state = default(MeshComposer<LineVertex, uint>.State);
			MeshComposer<LineVertex, uint> meshComposer = new MeshComposer<LineVertex, uint>(&state, in m_ProceduralMesh);
			PathSplineMeshBuilder.State state2 = default(PathSplineMeshBuilder.State);
			PathSplineMeshBuilder meshBuilder = new PathSplineMeshBuilder(&state2, m_PathLineSettings.thickness / 2f, meshComposer);
			PathSplinePlotterListener listener = new PathSplinePlotterListener(approximatePathBuilder: new ApproximatePathBuilder(ApproximatePath), meshBuilder: meshBuilder);
			meshComposer.StartSubMesh();
			meshBuilder.SetVertexPositionOffset(in positionOffset);
			SplinePlotter<PathSplineMetaData, PathSplinePlotterListener> splinePlotter = new SplinePlotter<PathSplineMetaData, PathSplinePlotterListener>(listener, m_BezierPoints, m_PathLineSettings.hardTurnSmoothDistanceFactor);
			new PathBuilder(splinePlotter, m_CellSize / 2f, m_PathLineSettings, m_Cells, m_CellIndices).Build();
			meshComposer.PushSubMesh((byte)m_MaterialId);
		}
	}

	private readonly GridSettings m_GridSettings;

	private readonly int m_MaterialId;

	private readonly PathLineSettings m_PathLineSettings;

	private readonly CustomGridGraph m_Graph;

	private readonly IAreaSource m_PathSource;

	private readonly ProceduralMesh<LineVertex, uint> m_ProceduralMesh;

	private readonly float3 m_PositionOffset;

	private readonly NativeList<ApproximatePathSegment> m_ApproximatePath;

	public BuildPathJob(GridSettings gridSettings, int materialId, PathLineSettings pathLineSettings, CustomGridGraph graph, IAreaSource pathSource, ProceduralMesh<LineVertex, uint> proceduralMesh, float3 positionOffset, NativeList<ApproximatePathSegment> approximatePath)
	{
		m_GridSettings = gridSettings;
		m_MaterialId = materialId;
		m_PathLineSettings = pathLineSettings;
		m_Graph = graph;
		m_PathSource = pathSource;
		m_ProceduralMesh = proceduralMesh;
		m_PositionOffset = positionOffset;
		m_ApproximatePath = approximatePath;
	}

	public JobHandle Schedule(JobHandle dependsOn = default(JobHandle))
	{
		var (nativeList, nativeList2) = CreateCells(Allocator.TempJob);
		if (nativeList.Length == 0)
		{
			nativeList.Dispose();
			nativeList2.Dispose();
			return dependsOn;
		}
		NativeArray<BezierPoint> nativeArray = new NativeArray<BezierPoint>(m_PathLineSettings.smoothSegmentsCount + 1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		JobHandle dependsOn2 = new ResolveCellsJob(nativeList, m_GridSettings).Schedule(dependsOn);
		BuildBezierPointsJob jobData = default(BuildBezierPointsJob);
		jobData.points = nativeArray;
		jobData.segmentsCount = m_PathLineSettings.smoothSegmentsCount;
		JobHandle dependsOn3 = jobData.Schedule(dependsOn2);
		Job jobData2 = default(Job);
		jobData2.m_CellSize = m_GridSettings.cellSize;
		jobData2.m_MaterialId = m_MaterialId;
		jobData2.m_PathLineSettings = m_PathLineSettings;
		jobData2.m_Cells = nativeList;
		jobData2.m_CellIndices = nativeList2;
		jobData2.m_BezierPoints = nativeArray;
		jobData2.m_ProceduralMesh = m_ProceduralMesh;
		jobData2.positionOffset = m_PositionOffset;
		jobData2.ApproximatePath = m_ApproximatePath;
		JobHandle inputDeps = jobData2.Schedule(dependsOn3);
		inputDeps = nativeList.Dispose(inputDeps);
		inputDeps = nativeList2.Dispose(inputDeps);
		return nativeArray.Dispose(inputDeps);
	}

	private (NativeList<CellUnion> cells, NativeList<int> cellIndices) CreateCells(Allocator allocator)
	{
		int num = m_PathSource.EstimateCount();
		NativeList<CellUnion> nativeList = new NativeList<CellUnion>(num, allocator);
		NativeList<int> nativeList2 = new NativeList<int>(num, allocator);
		ContainerAdapter container = new ContainerAdapter(num, m_Graph, nativeList, nativeList2);
		try
		{
			m_PathSource.GetCellIdentifiers(new Vector2Int(m_Graph.width, m_Graph.depth), ref container);
		}
		finally
		{
			container.Dispose();
		}
		return (cells: nativeList, cellIndices: nativeList2);
	}
}
