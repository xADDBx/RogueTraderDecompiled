using System.Collections.Generic;
using Kingmaker.Pathfinding;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

internal readonly struct SurfaceJobDataFactory
{
	public struct Result
	{
		public NativeList<CellUnion> cells;

		public NativeList<ushort> cellAreaMasks;

		public NativeList<ushort> chunkAreaMasks;

		public NativeList<StratagemDescriptor> stratagemDescriptorList;

		public NativeList<int> stratagemCellIndexList;
	}

	private struct ContainerAdapter : IIdentifierContainer
	{
		private readonly CustomGridGraph m_Graph;

		private readonly int m_GridDimensionX;

		private readonly int m_GridDimensionY;

		private readonly int m_ChunksCountX;

		private readonly int m_ChunksCountY;

		private NativeList<CellUnion> m_CellUnions;

		private NativeList<ushort> m_CellAreaMasks;

		private NativeList<ushort> m_ChunkAreaMasks;

		private NativeList<StratagemDescriptor> m_StratagemDescriptorList;

		private NativeList<int> m_StratagemCellIndexList;

		private ushort m_AreaFlag;

		private int m_IntersectionFlagShift;

		private bool m_CollectStratagemArea;

		private int m_StratagemAreaStartIndex;

		private unsafe int* m_ChunkIndicesPtr;

		private unsafe CellUnion* m_CellUnionsPtr;

		private unsafe ushort* m_CellAreaMasksPtr;

		private unsafe ushort* m_ChunkAreaMasksPtr;

		private int m_CellsCapacity;

		private int m_CellsCount;

		private int m_ChunksCapacity;

		private int m_ChunksCount;

		public unsafe ContainerAdapter(int estimatedCellsCount, CustomGridGraph graph, Allocator allocator)
		{
			m_Graph = graph;
			m_AreaFlag = 0;
			m_IntersectionFlagShift = 0;
			m_CollectStratagemArea = false;
			m_StratagemAreaStartIndex = 0;
			m_GridDimensionX = graph.width;
			m_GridDimensionY = graph.depth;
			m_ChunksCountX = math.max(1, (int)math.ceil((float)m_GridDimensionX / 8f));
			m_ChunksCountY = math.max(1, (int)math.ceil((float)m_GridDimensionY / 8f));
			int num = 4 * m_ChunksCountX * m_ChunksCountY;
			m_ChunkIndicesPtr = (int*)UnsafeUtility.Malloc(num, 4, Allocator.Temp);
			UnsafeUtility.MemSet(m_ChunkIndicesPtr, byte.MaxValue, num);
			int initialCapacity = num / 2;
			int initialCapacity2 = math.min(m_ChunksCountX * m_ChunksCountY * 64, math.max(1, estimatedCellsCount));
			m_CellUnions = new NativeList<CellUnion>(initialCapacity2, allocator);
			m_CellAreaMasks = new NativeList<ushort>(initialCapacity2, allocator);
			m_ChunkAreaMasks = new NativeList<ushort>(initialCapacity, allocator);
			m_StratagemDescriptorList = new NativeList<StratagemDescriptor>(allocator);
			m_StratagemCellIndexList = new NativeList<int>(allocator);
			m_CellsCount = 0;
			m_ChunksCount = 0;
			m_CellsCapacity = m_CellUnions.Capacity;
			m_ChunksCapacity = m_ChunkAreaMasks.Capacity;
			m_CellUnions.ResizeUninitialized(m_CellsCapacity);
			m_CellAreaMasks.ResizeUninitialized(m_CellsCapacity);
			m_ChunkAreaMasks.ResizeUninitialized(m_ChunksCapacity);
			m_CellUnionsPtr = m_CellUnions.GetUnsafePtr();
			m_CellAreaMasksPtr = m_CellAreaMasks.GetUnsafePtr();
			m_ChunkAreaMasksPtr = m_ChunkAreaMasks.GetUnsafePtr();
		}

		public unsafe void Dispose()
		{
			UnsafeUtility.Free(m_ChunkIndicesPtr, Allocator.Temp);
			if (m_CellUnions.IsCreated)
			{
				m_CellUnions.Dispose();
			}
			if (m_CellAreaMasks.IsCreated)
			{
				m_CellAreaMasks.Dispose();
			}
			if (m_ChunkAreaMasks.IsCreated)
			{
				m_ChunkAreaMasks.Dispose();
			}
			if (m_StratagemDescriptorList.IsCreated)
			{
				m_StratagemDescriptorList.Dispose();
			}
			if (m_StratagemCellIndexList.IsCreated)
			{
				m_StratagemCellIndexList.Dispose();
			}
		}

		public void Setup(ushort flag, int intersectionFlagShift, bool collectStratagemArea)
		{
			m_AreaFlag = flag;
			m_IntersectionFlagShift = intersectionFlagShift;
			m_CollectStratagemArea = collectStratagemArea;
			m_StratagemAreaStartIndex = m_StratagemCellIndexList.Length;
		}

		public void Commit()
		{
			if (m_CollectStratagemArea)
			{
				StratagemDescriptor stratagemDescriptor = default(StratagemDescriptor);
				stratagemDescriptor.start = m_StratagemAreaStartIndex;
				stratagemDescriptor.length = m_StratagemCellIndexList.Length - m_StratagemAreaStartIndex;
				StratagemDescriptor value = stratagemDescriptor;
				m_StratagemDescriptorList.Add(in value);
			}
		}

		public Result TakeResults()
		{
			try
			{
				m_CellUnions.ResizeUninitialized(m_CellsCount);
				m_CellAreaMasks.ResizeUninitialized(m_CellsCount);
				m_ChunkAreaMasks.ResizeUninitialized(m_ChunksCount);
				Result result = default(Result);
				result.cells = m_CellUnions;
				result.cellAreaMasks = m_CellAreaMasks;
				result.chunkAreaMasks = m_ChunkAreaMasks;
				result.stratagemDescriptorList = m_StratagemDescriptorList;
				result.stratagemCellIndexList = m_StratagemCellIndexList;
				return result;
			}
			finally
			{
				m_CellUnions = default(NativeList<CellUnion>);
				m_CellAreaMasks = default(NativeList<ushort>);
				m_ChunkAreaMasks = default(NativeList<ushort>);
				m_StratagemDescriptorList = default(NativeList<StratagemDescriptor>);
				m_StratagemCellIndexList = default(NativeList<int>);
			}
		}

		public void PushRange(int cellIdentifierBegin, int cellIdentifierEnd)
		{
			for (int i = cellIdentifierBegin; i < cellIdentifierEnd; i++)
			{
				Push(i);
			}
		}

		public unsafe void Push(int cellIdentifier)
		{
			int num = cellIdentifier % m_GridDimensionX;
			int num2 = cellIdentifier / m_GridDimensionX;
			int num3 = num / 8;
			int num4 = num2 / 8;
			int num5 = num3 + m_ChunksCountX * num4;
			int num6 = num % 8;
			int num7 = num2 % 8;
			int num8 = num6 + num7 * 8;
			int num9 = m_ChunkIndicesPtr[num5];
			if (num9 < 0)
			{
				num9 = m_ChunksCount++;
				m_ChunkIndicesPtr[num5] = num9;
				if (m_ChunksCount > m_ChunksCapacity)
				{
					GrowChunkContainers(m_ChunksCount);
				}
				int num10 = m_CellsCount + 64;
				if (num10 > m_CellsCapacity)
				{
					GrowCellContainers(num10);
				}
				CellUnion cellUnion = default(CellUnion);
				int num11 = num3 * 8;
				int num12 = num4 * 8;
				bool flag = false;
				int i = num12;
				for (int num13 = num12 + 8; i < num13; i++)
				{
					int j = num11;
					for (int num14 = num11 + 8; j < num14; j++)
					{
						ushort num16;
						if (j < m_GridDimensionX && i < m_GridDimensionY)
						{
							int num15 = j + m_GridDimensionX * i;
							CustomGridNode customGridNode = m_Graph.nodes[num15];
							CustomGridMeshNode customGridMeshNode = m_Graph.meshNodes[num15];
							cellUnion.intermediateCell.indexInGrid = num15;
							cellUnion.intermediateCell.packedHeight = customGridNode.position.y;
							cellUnion.intermediateCell.packedCornerOffsets.packedCornerOffsetNE = customGridMeshNode.packedCornerOffsetNE;
							cellUnion.intermediateCell.packedCornerOffsets.packedCornerOffsetNW = customGridMeshNode.packedCornerOffsetNW;
							cellUnion.intermediateCell.packedCornerOffsets.packedCornerOffsetSW = customGridMeshNode.packedCornerOffsetSW;
							cellUnion.intermediateCell.packedCornerOffsets.packedCornerOffsetSE = customGridMeshNode.packedCornerOffsetSE;
							cellUnion.intermediateCell.flags = (IntermediateCellFlags)(customGridNode.gridFlags & 0xFu);
							if ((customGridNode.flags & (true ? 1u : 0u)) != 0)
							{
								num16 = 1;
								flag = true;
							}
							else
							{
								cellUnion.intermediateCell.flags = (IntermediateCellFlags)0;
								num16 = 0;
							}
						}
						else
						{
							cellUnion.intermediateCell.indexInGrid = -1;
							num16 = 0;
						}
						m_CellUnionsPtr[m_CellsCount] = cellUnion;
						m_CellAreaMasksPtr[m_CellsCount] = num16;
						m_CellsCount++;
					}
				}
				m_ChunkAreaMasksPtr[num9] = (ushort)(flag ? 1u : 0u);
			}
			int value = num9 * 64 + num8;
			int num17 = m_CellAreaMasksPtr[value];
			int num18 = m_AreaFlag | ((num17 & m_AreaFlag) << m_IntersectionFlagShift);
			m_CellAreaMasksPtr[value] = (ushort)(num17 | num18);
			if (m_CollectStratagemArea)
			{
				m_StratagemCellIndexList.Add(in value);
			}
			int num19 = m_ChunkAreaMasksPtr[num9];
			int num20 = m_AreaFlag | ((num19 & m_AreaFlag) << m_IntersectionFlagShift);
			m_ChunkAreaMasksPtr[num9] = (ushort)(num19 | num20);
		}

		private unsafe void GrowChunkContainers(int minCapacity)
		{
			m_ChunkAreaMasks.SetCapacity(minCapacity);
			m_ChunksCapacity = m_ChunkAreaMasks.Capacity;
			m_ChunkAreaMasks.ResizeUninitialized(m_ChunksCapacity);
			m_ChunkAreaMasksPtr = m_ChunkAreaMasks.GetUnsafePtr();
		}

		private unsafe void GrowCellContainers(int minCapacity)
		{
			m_CellUnions.SetCapacity(minCapacity);
			m_CellAreaMasks.SetCapacity(minCapacity);
			m_CellsCapacity = m_CellUnions.Capacity;
			m_CellUnions.ResizeUninitialized(m_CellsCapacity);
			m_CellAreaMasks.ResizeUninitialized(m_CellsCapacity);
			m_CellUnionsPtr = m_CellUnions.GetUnsafePtr();
			m_CellAreaMasksPtr = m_CellAreaMasks.GetUnsafePtr();
		}
	}

	private const ushort kWalkableAreaFlag = 1;

	private readonly CustomGridGraph m_Graph;

	private readonly List<AreaData> m_Areas;

	public SurfaceJobDataFactory(CustomGridGraph graph, List<AreaData> areas)
	{
		m_Graph = graph;
		m_Areas = areas;
	}

	public Result Create(Allocator allocator)
	{
		try
		{
			int num = 0;
			foreach (AreaData area in m_Areas)
			{
				num += area.source.EstimateCount();
			}
			ContainerAdapter container = new ContainerAdapter(num, m_Graph, allocator);
			try
			{
				Vector2Int gridDimensions = new Vector2Int(m_Graph.width, m_Graph.depth);
				foreach (AreaData area2 in m_Areas)
				{
					container.Setup((ushort)area2.flag, area2.intersectionFlagShift, area2.isStratagem);
					area2.source.GetCellIdentifiers(gridDimensions, ref container);
					container.Commit();
				}
				return container.TakeResults();
			}
			finally
			{
				container.Dispose();
			}
		}
		finally
		{
		}
	}
}
