using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;

public class OptimizedSpatialHashingBroadphase : BroadphaseBase
{
	public const int kMaxHashtableLookupIterations = 100;

	private OptimizedSpatialHashingSettings m_Settings;

	private SpatialHashmapSoA m_SpatialHashmapSoA;

	private bool m_NeedClear;

	private NativeArray<float> m_LoadFactor;

	public override BroadphaseType Type => BroadphaseType.OptimizedSpatialHashing;

	public NativeArray<float> LoadFactor => m_LoadFactor;

	public SpatialHashmapSoA SpatialHashmapSoA => m_SpatialHashmapSoA;

	public OptimizedSpatialHashingBroadphase(OptimizedSpatialHashingSettings settings)
	{
		m_Settings = settings;
		m_SpatialHashmapSoA = new SpatialHashmapSoA(PBDMath.NextPrimeNumber(16000));
		m_LoadFactor = new NativeArray<float>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_LoadFactor[0] = 0f;
		m_NeedClear = true;
	}

	public override void ResizeBuffers(int collidersCount, int bodiesCount, int forceVolumesCount)
	{
		base.ResizeBuffers(collidersCount, bodiesCount, forceVolumesCount);
		if (m_LoadFactor[0] > 0.3f)
		{
			m_SpatialHashmapSoA.Resize(PBDMath.NextPrimeNumber(m_SpatialHashmapSoA.Length));
			m_NeedClear = true;
		}
	}

	public override JobHandle Update(JobHandle lastJobHandle)
	{
		if (m_NeedClear)
		{
			ClearSpatialHashmapJob jobData = default(ClearSpatialHashmapJob);
			jobData.SpatialHashFrameId = m_SpatialHashmapSoA.FrameId;
			jobData.BodyColliderPairs = base.BodyCollidersPairsSoA.Array;
			jobData.BodyForceVolumePairs = base.BodyForceVolumePairsSoA.Array;
			lastJobHandle = IJobParallelForExtensions.Schedule(jobData, m_SpatialHashmapSoA.Length, 1, lastJobHandle);
			m_NeedClear = false;
		}
		SpatialHashmapBuildJob jobData2 = default(SpatialHashmapBuildJob);
		jobData2.Count = m_BodiesCount + m_CollidersCount + m_ForceVolumesCount;
		jobData2.CellSize = m_Settings.CellSize;
		jobData2.InvCellSize = math.rcp(m_Settings.CellSize);
		jobData2.FrameId = (uint)Time.frameCount;
		jobData2.HashtableSize = (uint)m_SpatialHashmapSoA.Length;
		jobData2.AabbMin = base.AabbSoA.AabbMin;
		jobData2.AabbMax = base.AabbSoA.AabbMax;
		jobData2.SpatialHashtableKeys = m_SpatialHashmapSoA.Keys;
		jobData2.SpatialHashmapValues = m_SpatialHashmapSoA.Values;
		jobData2.SpatialHashmapFrameId = m_SpatialHashmapSoA.FrameId;
		lastJobHandle = jobData2.Schedule(lastJobHandle);
		SpatialHashmapFindPairsJob jobData3 = default(SpatialHashmapFindPairsJob);
		jobData3.Offset = base.BodiesAabbOffset;
		jobData3.CellSize = m_Settings.CellSize;
		jobData3.InvCellSize = math.rcp(m_Settings.CellSize);
		jobData3.HashtableSize = (uint)m_SpatialHashmapSoA.Length;
		jobData3.FrameId = (uint)Time.frameCount;
		jobData3.SpatialHashmapKeys = m_SpatialHashmapSoA.Keys;
		jobData3.SpatialHashmapValues = m_SpatialHashmapSoA.Values;
		jobData3.SpatialHashmapFrameId = m_SpatialHashmapSoA.FrameId;
		jobData3.CollidersRange = new uint2(0u, (uint)m_CollidersCount);
		jobData3.ForceVolumesRange = new uint2((uint)m_CollidersCount, (uint)(m_CollidersCount + m_ForceVolumesCount));
		jobData3.AabbMin = base.AabbSoA.AabbMin;
		jobData3.AabbMax = base.AabbSoA.AabbMax;
		jobData3.BodyColliderPairs = base.BodyCollidersPairsSoA.Array;
		jobData3.BodyForceVolumePairs = base.BodyForceVolumePairsSoA.Array;
		lastJobHandle = IJobParallelForExtensions.Schedule(jobData3, m_BodiesCount, 1, lastJobHandle);
		SpatialHasmapLoadFactorJob jobData4 = default(SpatialHasmapLoadFactorJob);
		jobData4.FrameId = (uint)Time.frameCount;
		jobData4.HashtableCapacity = (uint)m_SpatialHashmapSoA.Length;
		jobData4.LoadFactor = m_LoadFactor;
		jobData4.SpatialHashtableFrameId = m_SpatialHashmapSoA.FrameId;
		lastJobHandle = jobData4.Schedule(lastJobHandle);
		return lastJobHandle;
	}

	public override void Dispose()
	{
		base.Dispose();
		m_SpatialHashmapSoA.Dispose();
		m_LoadFactor.Dispose();
	}
}
