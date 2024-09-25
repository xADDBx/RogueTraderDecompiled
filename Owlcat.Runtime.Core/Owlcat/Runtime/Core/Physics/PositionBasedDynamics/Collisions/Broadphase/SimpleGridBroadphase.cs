using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;
using Unity.Jobs;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;

public class SimpleGridBroadphase : BroadphaseBase
{
	public const int kMaxEntriesPerCellInGrid = 17;

	public const int kMaxGlobalCollidersCount = 256;

	private SimpleGridSettings m_Settings;

	public override BroadphaseType Type => BroadphaseType.SimpleGrid;

	public SimpleGridBroadphase(SimpleGridSettings settings)
	{
		m_Settings = settings;
	}

	public override void ResizeBuffers(int collidersCount, int bodiesCount, int forceVolumesCount)
	{
		m_CollidersCount = collidersCount;
		m_BodiesCount = bodiesCount;
		m_ForceVolumesCount = forceVolumesCount;
		ResizeSoA(base.AabbSoA, m_CollidersCount + m_BodiesCount + m_ForceVolumesCount);
		int num = CalculateGridSize();
		if (num != m_BodyColliderPairsSoA.Length)
		{
			ResizeSoA(m_BodyColliderPairsSoA, num);
			ResizeSoA(m_BodyForceVolumePairsSoA, num);
		}
	}

	public override JobHandle Update(JobHandle lastJobHandle)
	{
		if (m_CollidersCount > 256)
		{
			Debug.LogError($"The limit ({256}) on the number of global colliders has been exceeded");
		}
		UpdateCollidersGridJob jobData = default(UpdateCollidersGridJob);
		jobData.CollidersAabbMin = base.AabbSoA.AabbMin;
		jobData.CollidersAabbMax = base.AabbSoA.AabbMax;
		jobData.GlobalCollidersCount = m_CollidersCount;
		jobData.GridResolution = m_Settings.GridResolution;
		jobData.Grid = m_BodyColliderPairsSoA.Array;
		jobData.GridAabb = base.SceneAabb;
		lastJobHandle = jobData.Schedule(lastJobHandle);
		UpdateForceVolumesGridJob jobData2 = default(UpdateForceVolumesGridJob);
		jobData2.ForceVolumesAabbMin = base.AabbSoA.AabbMin;
		jobData2.ForceVolumesAabbMax = base.AabbSoA.AabbMax;
		jobData2.AabbOffset = base.ForceVolumesAabbOffset;
		jobData2.ForceVolumesCount = m_ForceVolumesCount;
		jobData2.GridResolution = m_Settings.GridResolution;
		jobData2.Grid = m_BodyForceVolumePairsSoA.Array;
		jobData2.GridAabb = base.SceneAabb;
		lastJobHandle = jobData2.Schedule(lastJobHandle);
		return lastJobHandle;
	}

	private int CalculateGridSize()
	{
		int num = 17;
		return m_Settings.GridResolution * m_Settings.GridResolution * num;
	}
}
