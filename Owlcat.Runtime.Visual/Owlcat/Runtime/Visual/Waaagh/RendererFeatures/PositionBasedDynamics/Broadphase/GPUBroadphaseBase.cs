using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics.Broadphase;

public abstract class GPUBroadphaseBase
{
	public const int kMaxColliderPairsPerBody = 16;

	public const int kMaxForceVolumePairsPerBody = 8;

	protected int m_CollidersCount;

	protected int m_BodiesCount;

	protected int m_ForceVolumesCount;

	protected GPUBoundingBoxSoA m_BoundingBoxSoA;

	protected GPUSingleBufferSoA<int> m_BodyColliderPairsSoA;

	protected GPUSingleBufferSoA<int> m_BodyForceVolumePairsSoA;

	protected GPUSingleBufferSoA<float3> m_SceneAabbSoA;

	public int CollidersCount => m_CollidersCount;

	public int ForceVolumesCount => m_ForceVolumesCount;

	public int BodiesCount => m_BodiesCount;

	public int CollidersAabbOffset => 0;

	public int ForceVolumesAabbOffset => m_CollidersCount;

	public int BodiesAabbOffset => m_CollidersCount + m_ForceVolumesCount;

	public GPUBoundingBoxSoA BoundingBoxSoA => m_BoundingBoxSoA;

	public GPUSingleBufferSoA<float3> SceneAabbSoA => m_SceneAabbSoA;

	public GPUSingleBufferSoA<int> BodyColliderPairsSoA => m_BodyColliderPairsSoA;

	public GPUSingleBufferSoA<int> BodyForceVolumePairsSoA => m_BodyForceVolumePairsSoA;

	public abstract BroadphaseType Type { get; }

	public GPUBroadphaseBase()
	{
		m_BoundingBoxSoA = new GPUBoundingBoxSoA();
		m_BodyColliderPairsSoA = new GPUSingleBufferSoA<int>("_PbdBodyColliderPairsBuffer");
		m_BodyForceVolumePairsSoA = new GPUSingleBufferSoA<int>("_PbdBodyForceVolumePairsBuffer");
		m_SceneAabbSoA = new GPUSingleBufferSoA<float3>("_PbdBroadphaseSceneAabbBuffer", 4);
	}

	public virtual void Dispose()
	{
		m_BoundingBoxSoA.Dispose();
		m_BodyColliderPairsSoA.Dispose();
		m_BodyForceVolumePairsSoA.Dispose();
		m_SceneAabbSoA.Dispose();
	}

	public virtual void ResizeBuffers(int collidersCount, int bodiesCount, int forceVolumesCount)
	{
		m_CollidersCount = collidersCount;
		m_BodiesCount = bodiesCount;
		m_ForceVolumesCount = forceVolumesCount;
		int newSize = math.max(1, m_CollidersCount + m_BodiesCount + m_ForceVolumesCount);
		ResizeSoA(m_BoundingBoxSoA, newSize);
		int newSize2 = math.max(1, m_BodiesCount * 16);
		ResizeSoA(m_BodyColliderPairsSoA, newSize2);
		int newSize3 = math.max(1, m_BodiesCount * 8);
		ResizeSoA(m_BodyForceVolumePairsSoA, newSize3);
	}

	protected void ResizeSoA(GPUSoABase soa, int newSize)
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

	public abstract void Update(CommandBuffer cmd);
}
