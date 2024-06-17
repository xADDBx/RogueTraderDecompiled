using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Broadphase;

public class GPUSimpleGridBroadphase : GPUBroadphaseBase
{
	public const int kMaxEntriesPerCellInGrid = 17;

	public const int kMaxGlobalCollidersCount = 256;

	private SimpleGridSettings m_Settings;

	private ComputeShader m_CollisionShader;

	private int m_KernelClearCollidersGrid;

	private int m_KernelUpdateCollidersGrid;

	private ComputeShader m_ForceVolumeShader;

	private int m_KernelClearForceVolumesGrid;

	private int m_KernelUpdateForceFolumesGrid;

	public override BroadphaseType Type => BroadphaseType.SimpleGrid;

	public GPUSimpleGridBroadphase(SimpleGridSettings settings, ComputeShader collisionShader, ComputeShader forceVolumeShader)
	{
		m_Settings = settings;
		m_CollisionShader = collisionShader;
		m_KernelClearCollidersGrid = m_CollisionShader.FindKernel("ClearCollidersGrid");
		m_KernelUpdateCollidersGrid = m_CollisionShader.FindKernel("UpdateCollidersGrid");
		m_ForceVolumeShader = forceVolumeShader;
		m_KernelClearForceVolumesGrid = m_ForceVolumeShader.FindKernel("ClearForceVolumesGrid");
		m_KernelUpdateForceFolumesGrid = m_ForceVolumeShader.FindKernel("UpdateForceFolumesGrid");
	}

	public override void ResizeBuffers(int collidersCount, int bodiesCount, int forceVolumesCount)
	{
		m_CollidersCount = collidersCount;
		m_BodiesCount = bodiesCount;
		m_ForceVolumesCount = forceVolumesCount;
		ResizeSoA(base.BoundingBoxSoA, m_CollidersCount + m_BodiesCount + m_ForceVolumesCount);
		int num = CalculateGridSize();
		if (num != m_BodyColliderPairsSoA.Length)
		{
			ResizeSoA(m_BodyColliderPairsSoA, num);
			ResizeSoA(m_BodyForceVolumePairsSoA, num);
		}
	}

	public override void Update(CommandBuffer cmd)
	{
		cmd.SetComputeIntParam(m_CollisionShader, PositionBasedDynamicsConstantBuffer._BroadphaseGridResolution, m_Settings.GridResolution);
		cmd.SetComputeIntParam(m_ForceVolumeShader, PositionBasedDynamicsConstantBuffer._ForceVolumesAabbOffset, base.ForceVolumesAabbOffset);
		m_SceneAabbSoA.SetGlobalData(cmd);
		m_BodyColliderPairsSoA.SetGlobalData(cmd);
		m_BodyForceVolumePairsSoA.SetGlobalData(cmd);
		cmd.DispatchCompute(m_CollisionShader, m_KernelClearCollidersGrid, RenderingUtils.DivRoundUp(m_BodyColliderPairsSoA.Length, 64), 1, 1);
		if (m_CollidersCount > 0)
		{
			cmd.SetComputeIntParam(m_CollisionShader, PositionBasedDynamicsConstantBuffer._GlobalCollidersCount, m_CollidersCount);
			cmd.DispatchCompute(m_CollisionShader, m_KernelUpdateCollidersGrid, 1, 1, 1);
		}
		cmd.DispatchCompute(m_ForceVolumeShader, m_KernelClearForceVolumesGrid, RenderingUtils.DivRoundUp(m_BodyForceVolumePairsSoA.Length, 64), 1, 1);
		if (m_ForceVolumesCount > 0)
		{
			cmd.SetComputeIntParam(m_ForceVolumeShader, PositionBasedDynamicsConstantBuffer._Count, m_ForceVolumesCount);
			cmd.DispatchCompute(m_ForceVolumeShader, m_KernelUpdateForceFolumesGrid, 1, 1, 1);
		}
	}

	private int CalculateGridSize()
	{
		int num = 17;
		return m_Settings.GridResolution * m_Settings.GridResolution * num;
	}
}
