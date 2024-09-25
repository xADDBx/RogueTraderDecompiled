using System.Collections.Generic;
using Kingmaker.Pathfinding;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

internal struct BuildSurfaceJob
{
	[BurstCompile]
	private struct Job : IJob
	{
		public float cellSize;

		public float borderCutSize;

		public float borderFadeSize;

		public bool mergeFillSubMeshes;

		public bool mergeOutlineSubMeshes;

		[ReadOnly]
		public NativeArray<CellUnion> cellArray;

		[ReadOnly]
		public NativeArray<ushort> cellAreaMaskArray;

		[ReadOnly]
		public NativeArray<ushort> chunkAreaMasks;

		[ReadOnly]
		public StratagemBuffer stratagemBuffer;

		[ReadOnly]
		public CommandBuffer commandBuffer;

		public NativeArray<byte> surfaceFragmentArray;

		public NativeArray<byte> surfaceChunkArray;

		public NativeArray<byte> edgeSegmentBuffer;

		public NativeList<OutlinePlotCommand> m_OutlinePlotCommands;

		public NativeHashMap<byte, MaterialAreaDescriptor> materialIdToMaterialAreaDescriptorMap;

		public ProceduralMesh<FillVertex, uint> surfaceProceduralMesh;

		public ProceduralMesh<LineVertex, uint> outlineProceduralMesh;

		public float outlineTurnRadius;

		public float outlineLineThickness;

		public NativeArray<BezierPoint> outlineTurnBezierPoints;

		public NativeArray<byte> outlineProcessStatusFlags;

		public unsafe void Execute()
		{
			CellBuffer cellBuffer = new CellBuffer(cellArray, cellAreaMaskArray);
			FillBuffer fillBuffer = new FillBuffer(surfaceFragmentArray, surfaceChunkArray);
			MaterialAreaDescriptorBuffer materialAreaDescriptorBuffer = new MaterialAreaDescriptorBuffer(materialIdToMaterialAreaDescriptorMap);
			FillBufferWriter fillBufferWriter = new FillBufferWriter(cellBuffer, fillBuffer, materialAreaDescriptorBuffer);
			FillMeshBuilder fillMeshBuilder = new FillMeshBuilder(cellSize, cutSize: borderCutSize, fadeSize: borderFadeSize, cellBuffer: cellBuffer, fillBuffer: fillBuffer, materialAreaDescriptorBuffer: materialAreaDescriptorBuffer, proceduralMesh: surfaceProceduralMesh);
			MeshComposer<LineVertex, uint>.State state = default(MeshComposer<LineVertex, uint>.State);
			MeshComposer<LineVertex, uint> meshComposer = new MeshComposer<LineVertex, uint>(&state, in outlineProceduralMesh);
			OutlineSplineMeshBuilder.State state2 = default(OutlineSplineMeshBuilder.State);
			OutlineSplineMeshBuilder splineMeshBuilder = new OutlineSplineMeshBuilder(&state2, outlineLineThickness / 2f, meshComposer);
			OutlineBuilder outlineBuilder = new OutlineBuilder(cellBuffer, chunkAreaMasks, cellSize / 2f, outlineTurnRadius, outlineTurnBezierPoints, outlineProcessStatusFlags, m_OutlinePlotCommands);
			foreach (CommandRecord record in commandBuffer.recordList)
			{
				switch (record.code)
				{
				case CommandCode.WriteFill:
				{
					WriteFillCommandData writeFillCommandData = commandBuffer.writeFillCommandDataList[record.dataIndex];
					if (writeFillCommandData.shapeId >= 0)
					{
						fillBufferWriter.Write(writeFillCommandData.materialId, stratagemBuffer.GetCellIndices(writeFillCommandData.shapeId), writeFillCommandData.selectFilter);
					}
					else
					{
						fillBufferWriter.Write(writeFillCommandData.materialId, writeFillCommandData.selectFilter);
					}
					break;
				}
				case CommandCode.ClearFillBuffer:
					fillBuffer.Clear();
					materialAreaDescriptorBuffer.Clear();
					break;
				case CommandCode.ClearOutlineBuffer:
					UnsafeUtility.MemClear(edgeSegmentBuffer.GetUnsafePtr(), edgeSegmentBuffer.Length);
					break;
				case CommandCode.BuildFill:
				{
					float3 layerOffset = commandBuffer.buildFillCommandDataList[record.dataIndex].meshOffset;
					fillMeshBuilder.Build(in layerOffset);
					break;
				}
				case CommandCode.ComposeOutlineMesh:
				{
					ComposeOutlineMeshCommandData composeOutlineMeshCommandData = commandBuffer.composeOutlineMeshCommandDataList[record.dataIndex];
					OutlineCellFilter shape = new OutlineCellFilter(cellBuffer, fillBuffer, composeOutlineMeshCommandData.shape);
					OutlineCellFilter shapeMask = new OutlineCellFilter(cellBuffer, fillBuffer, composeOutlineMeshCommandData.mask);
					EdgeBuffer edgeBuffer = new EdgeBuffer(cellBuffer, edgeSegmentBuffer, composeOutlineMeshCommandData.overwrite);
					meshComposer.StartSubMesh();
					splineMeshBuilder.SetVertexPositionOffset(in composeOutlineMeshCommandData.meshOffset);
					outlineBuilder.Execute(composeOutlineMeshCommandData.lineType, in shape, in shapeMask, ref edgeBuffer, ref splineMeshBuilder);
					break;
				}
				case CommandCode.AppendOutlineMesh:
				{
					AppendMeshCommandData appendMeshCommandData = commandBuffer.appendMeshCommandDataList[record.dataIndex];
					if (mergeOutlineSubMeshes)
					{
						meshComposer.PushSubMeshMerged((byte)appendMeshCommandData.materialId);
					}
					else
					{
						meshComposer.PushSubMesh((byte)appendMeshCommandData.materialId);
					}
					break;
				}
				}
			}
			if (mergeFillSubMeshes)
			{
				surfaceProceduralMesh.MergeSubMeshes();
			}
			if (mergeOutlineSubMeshes)
			{
				outlineProceduralMesh.MergeSubMeshes();
			}
		}
	}

	private readonly OutlineSettings m_BorderLineSettings;

	private readonly FillSettings m_FillSettings;

	private readonly CustomGridGraph m_Graph;

	private readonly List<AreaData> m_ImportedAreas;

	private readonly CommandBuffer m_CommandBuffer;

	private readonly ProceduralMesh<FillVertex, uint> m_SurfaceProceduralMesh;

	private readonly ProceduralMesh<LineVertex, uint> m_OutlineProceduralMesh;

	public BuildSurfaceJob(OutlineSettings borderLineSettings, FillSettings fillSettings, CustomGridGraph graph, List<AreaData> importedAreas, CommandBuffer commandBuffer, ProceduralMesh<FillVertex, uint> surfaceProceduralMesh, ProceduralMesh<LineVertex, uint> outlineProceduralMesh)
	{
		m_BorderLineSettings = borderLineSettings;
		m_FillSettings = fillSettings;
		m_Graph = graph;
		m_ImportedAreas = importedAreas;
		m_CommandBuffer = commandBuffer;
		m_SurfaceProceduralMesh = surfaceProceduralMesh;
		m_OutlineProceduralMesh = outlineProceduralMesh;
	}

	public JobHandle Schedule(JobHandle dependsOn = default(JobHandle))
	{
		GridSettings gridSettings = new GridSettings(m_Graph);
		SurfaceJobDataFactory.Result result = new SurfaceJobDataFactory(m_Graph, m_ImportedAreas).Create(Allocator.TempJob);
		NativeArray<byte> surfaceFragmentArray = new NativeArray<byte>(result.cells.Length, Allocator.TempJob);
		NativeArray<byte> surfaceChunkArray = new NativeArray<byte>(result.cells.Length / 64, Allocator.TempJob);
		NativeArray<byte> edgeSegmentBuffer = new NativeArray<byte>(result.cells.Length, Allocator.TempJob);
		NativeHashMap<byte, MaterialAreaDescriptor> materialIdToMaterialAreaDescriptorMap = new NativeHashMap<byte, MaterialAreaDescriptor>(16, Allocator.TempJob);
		NativeArray<BezierPoint> nativeArray = new NativeArray<BezierPoint>(m_BorderLineSettings.turnSmoothSegmentsCount + 1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<byte> outlineProcessStatusFlags = new NativeArray<byte>(result.cells.Length, Allocator.TempJob);
		NativeList<OutlinePlotCommand> outlinePlotCommands = new NativeList<OutlinePlotCommand>(Allocator.TempJob);
		JobHandle dependsOn2 = new ResolveCellsJob(result.cells, gridSettings).Schedule(dependsOn);
		BuildBezierPointsJob jobData = default(BuildBezierPointsJob);
		jobData.points = nativeArray;
		jobData.segmentsCount = m_BorderLineSettings.turnSmoothSegmentsCount;
		JobHandle jobHandle = jobData.Schedule(dependsOn2);
		Job jobData2 = new Job
		{
			cellSize = gridSettings.cellSize,
			borderCutSize = m_FillSettings.borderCutSize,
			borderFadeSize = m_FillSettings.borderFadeSize,
			mergeFillSubMeshes = m_FillSettings.mergeSubMeshes,
			mergeOutlineSubMeshes = m_BorderLineSettings.mergeSubMeshes,
			cellArray = result.cells,
			cellAreaMaskArray = result.cellAreaMasks,
			commandBuffer = m_CommandBuffer,
			surfaceFragmentArray = surfaceFragmentArray,
			surfaceChunkArray = surfaceChunkArray,
			edgeSegmentBuffer = edgeSegmentBuffer,
			materialIdToMaterialAreaDescriptorMap = materialIdToMaterialAreaDescriptorMap,
			surfaceProceduralMesh = m_SurfaceProceduralMesh,
			outlineProceduralMesh = m_OutlineProceduralMesh,
			outlineTurnRadius = m_BorderLineSettings.turnSmoothDistance,
			outlineLineThickness = m_BorderLineSettings.lineThickness,
			outlineTurnBezierPoints = nativeArray,
			outlineProcessStatusFlags = outlineProcessStatusFlags,
			m_OutlinePlotCommands = outlinePlotCommands,
			chunkAreaMasks = result.chunkAreaMasks,
			stratagemBuffer = new StratagemBuffer
			{
				descriptorList = result.stratagemDescriptorList,
				cellIndexList = result.stratagemCellIndexList
			}
		};
		JobHandle dependsOn3 = jobHandle;
		dependsOn3 = jobData2.Schedule(dependsOn3);
		dependsOn3 = result.cells.Dispose(dependsOn3);
		dependsOn3 = result.cellAreaMasks.Dispose(dependsOn3);
		dependsOn3 = surfaceFragmentArray.Dispose(dependsOn3);
		dependsOn3 = surfaceChunkArray.Dispose(dependsOn3);
		dependsOn3 = materialIdToMaterialAreaDescriptorMap.Dispose(dependsOn3);
		dependsOn3 = nativeArray.Dispose(dependsOn3);
		dependsOn3 = outlineProcessStatusFlags.Dispose(dependsOn3);
		dependsOn3 = edgeSegmentBuffer.Dispose(dependsOn3);
		dependsOn3 = outlinePlotCommands.Dispose(dependsOn3);
		dependsOn3 = result.chunkAreaMasks.Dispose(dependsOn3);
		dependsOn3 = result.stratagemDescriptorList.Dispose(dependsOn3);
		return result.stratagemCellIndexList.Dispose(dependsOn3);
	}
}
