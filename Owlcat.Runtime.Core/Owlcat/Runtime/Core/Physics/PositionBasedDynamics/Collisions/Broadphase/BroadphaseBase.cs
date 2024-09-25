using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase.Jobs;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;

public abstract class BroadphaseBase
{
	public const int kMaxColliderPairsPerBody = 16;

	public const int kMaxForceVolumePairsPerBody = 8;

	protected NativeArray<float3> m_SceneAabb;

	protected SingleArraySoA<int> m_BodyColliderPairsSoA;

	protected SingleArraySoA<int> m_BodyForceVolumePairsSoA;

	protected int m_CollidersCount;

	protected int m_BodiesCount;

	protected int m_ForceVolumesCount;

	public int CollidersCount => m_CollidersCount;

	public int ForceVolumesCount => m_ForceVolumesCount;

	public int BodiesCount => m_BodiesCount;

	public int CollidersAabbOffset => 0;

	public int ForceVolumesAabbOffset => m_CollidersCount;

	public int BodiesAabbOffset => m_CollidersCount + m_ForceVolumesCount;

	public BoundingBoxSoA AabbSoA { get; private set; }

	public NativeArray<float3> SceneAabb => m_SceneAabb;

	public SingleArraySoA<int> BodyCollidersPairsSoA => m_BodyColliderPairsSoA;

	public SingleArraySoA<int> BodyForceVolumePairsSoA => m_BodyForceVolumePairsSoA;

	public abstract BroadphaseType Type { get; }

	public BroadphaseBase()
	{
		AabbSoA = new BoundingBoxSoA();
		m_SceneAabb = new NativeArray<float3>(4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_BodyColliderPairsSoA = new SingleArraySoA<int>(1);
		m_BodyForceVolumePairsSoA = new SingleArraySoA<int>(1);
	}

	public virtual void Dispose()
	{
		AabbSoA.Dispose();
		m_SceneAabb.Dispose();
		m_BodyColliderPairsSoA.Dispose();
		m_BodyForceVolumePairsSoA.Dispose();
	}

	public virtual void ResizeBuffers(int collidersCount, int bodiesCount, int forceVolumesCount)
	{
		m_CollidersCount = collidersCount;
		m_BodiesCount = bodiesCount;
		m_ForceVolumesCount = forceVolumesCount;
		ResizeSoA(AabbSoA, m_CollidersCount + m_BodiesCount + m_ForceVolumesCount);
		ResizeSoA(m_BodyColliderPairsSoA, m_BodiesCount * 16);
		ResizeSoA(m_BodyForceVolumePairsSoA, m_BodiesCount * 8);
	}

	protected void ResizeSoA(StructureOfArrayBase soa, int newSize)
	{
		if (soa.Length < newSize)
		{
			soa.Resize((int)((float)newSize * 1.5f));
		}
		else if (soa.Length > (int)((float)newSize * 1.5f))
		{
			soa.Resize(newSize);
		}
	}

	protected JobHandle CalculateSceneAabb(JobHandle lastJobHandle, int count)
	{
		CalculateSceneAabbJob jobData = default(CalculateSceneAabbJob);
		jobData.Count = count;
		jobData.AabbMin = AabbSoA.AabbMin;
		jobData.AabbMax = AabbSoA.AabbMax;
		jobData.SceneAabb = m_SceneAabb;
		lastJobHandle = jobData.Schedule(lastJobHandle);
		return lastJobHandle;
	}

	public abstract JobHandle Update(JobHandle lastJobHandle);
}
