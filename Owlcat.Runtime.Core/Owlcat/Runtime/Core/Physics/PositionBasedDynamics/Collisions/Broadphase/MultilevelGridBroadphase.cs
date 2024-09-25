using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase.Jobs;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;

public class MultilevelGridBroadphase : BroadphaseBase
{
	public const int kMultilevelGridLevelCount = 4;

	private MultilevelGridSettings m_Settings;

	private SingleArraySoA<KeyValuePairComparable<uint, uint>> m_HashSoA;

	private NativeHashMap<uint, uint> m_CellStart;

	public float CellSize => m_Settings.CellSize;

	public NativeHashMap<uint, uint> CellStart => m_CellStart;

	public override BroadphaseType Type => BroadphaseType.MultilevelGrid;

	public MultilevelGridBroadphase(MultilevelGridSettings settings)
	{
		m_Settings = settings;
		m_HashSoA = new SingleArraySoA<KeyValuePairComparable<uint, uint>>(1);
		m_CellStart = new NativeHashMap<uint, uint>(1, Allocator.Persistent);
	}

	public override void Dispose()
	{
		base.Dispose();
		m_HashSoA.Dispose();
		m_CellStart.Dispose();
	}

	public override void ResizeBuffers(int collidersCount, int bodiesCount, int forceVolumesCount)
	{
		base.ResizeBuffers(collidersCount, bodiesCount, forceVolumesCount);
		int num = collidersCount + bodiesCount + forceVolumesCount;
		int num2 = 1;
		int dimension = (int)m_Settings.Dimension;
		int newSize = num * num2 * (1 << dimension);
		ResizeSoA(m_HashSoA, newSize);
		int num3 = collidersCount + forceVolumesCount + bodiesCount;
		if (m_CellStart.Capacity < num3)
		{
			m_CellStart.Dispose();
			m_CellStart = new NativeHashMap<uint, uint>((int)((float)num3 * 1.5f), Allocator.Persistent);
		}
		else if (m_CellStart.Capacity > (int)((float)num3 * 1.5f))
		{
			m_CellStart.Dispose();
			m_CellStart = new NativeHashMap<uint, uint>(num3, Allocator.Persistent);
		}
	}

	public override JobHandle Update(JobHandle lastJobHandle)
	{
		int num = m_CollidersCount + m_ForceVolumesCount + m_BodiesCount;
		int num2 = 1 << (int)m_Settings.Dimension;
		lastJobHandle = CalculateSceneAabb(lastJobHandle, num);
		GridCalculateHashJob jobData = default(GridCalculateHashJob);
		jobData.CellSize = new float3(CellSize);
		jobData.Dimension = m_Settings.Dimension;
		jobData.AabbMin = base.AabbSoA.AabbMin;
		jobData.AabbMax = base.AabbSoA.AabbMax;
		jobData.SceneAabb = m_SceneAabb;
		jobData.Hash = m_HashSoA.Array;
		lastJobHandle = IJobParallelForExtensions.Schedule(jobData, num, 1, lastJobHandle);
		QuickSortJob<KeyValuePairComparable<uint, uint>> jobData2 = default(QuickSortJob<KeyValuePairComparable<uint, uint>>);
		jobData2.Entries = m_HashSoA.Array;
		jobData2.StartIndex = 0;
		jobData2.EndIndex = num * num2;
		lastJobHandle = jobData2.Schedule(lastJobHandle);
		GridFindCellStartJob jobData3 = default(GridFindCellStartJob);
		jobData3.CellStart = m_CellStart;
		jobData3.Count = num * num2;
		jobData3.Hash = m_HashSoA.Array;
		lastJobHandle = jobData3.Schedule(lastJobHandle);
		GridFindOverlappingPairsJob jobData4 = default(GridFindOverlappingPairsJob);
		jobData4.Offset = base.BodiesAabbOffset;
		jobData4.CellSize = CellSize;
		jobData4.CollidersRange = new uint2(0u, (uint)m_CollidersCount);
		jobData4.ForceVolumesRange = new uint2((uint)m_CollidersCount, (uint)(m_CollidersCount + m_ForceVolumesCount));
		jobData4.SceneAabb = base.SceneAabb;
		jobData4.HashCount = m_HashSoA.Array.Length;
		jobData4.AabbMin = base.AabbSoA.AabbMin;
		jobData4.AabbMax = base.AabbSoA.AabbMax;
		jobData4.CellStart = m_CellStart;
		jobData4.Hash = m_HashSoA.Array;
		jobData4.BodyColliderPairs = m_BodyColliderPairsSoA.Array;
		jobData4.BodyForceVolumePairs = m_BodyForceVolumePairsSoA.Array;
		lastJobHandle = IJobParallelForExtensions.Schedule(jobData4, m_BodiesCount, 1, lastJobHandle);
		return lastJobHandle;
	}
}
