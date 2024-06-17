using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics.Broadphase;

public class GPUOptimizedSpatialHashingBroadphase : GPUBroadphaseBase
{
	public const int kMaxHashtableLookupIterations = 100;

	private static int _HashtableCapacity = Shader.PropertyToID("_HashtableCapacity");

	private static int _BodyColliderPairsCapacity = Shader.PropertyToID("_BodyColliderPairsCapacity");

	private static int _BodyForceVolumePairsCapacity = Shader.PropertyToID("_BodyForceVolumePairsCapacity");

	private static int _AabbCount = Shader.PropertyToID("_AabbCount");

	private static int _InvCellSize = Shader.PropertyToID("_InvCellSize");

	private static int _BodiesAabbCount = Shader.PropertyToID("_BodiesAabbCount");

	private static int _BodiesAabbOffset = Shader.PropertyToID("_BodiesAabbOffset");

	private static int _ColliderAabbCount = Shader.PropertyToID("_ColliderAabbCount");

	private static int _ColliderAabbOffset = Shader.PropertyToID("_ColliderAabbOffset");

	private static int _ForceVolumeAabbCount = Shader.PropertyToID("_ForceVolumeAabbCount");

	private static int _ForceVolumeAabbOffset = Shader.PropertyToID("_ForceVolumeAabbOffset");

	private OptimizedSpatialHashingSettings m_Settings;

	private ComputeShader m_SpatialHashingShader;

	private int m_KernelClear;

	private int m_KernelBuildHash;

	private int m_KernelFindPairs;

	private GPUSpatialHashmapSoA m_SpatialHashmapSoA;

	public override BroadphaseType Type => BroadphaseType.OptimizedSpatialHashing;

	public GPUOptimizedSpatialHashingBroadphase(OptimizedSpatialHashingSettings settings, ComputeShader spatialHashingShader)
	{
		m_Settings = settings;
		m_SpatialHashingShader = spatialHashingShader;
		m_KernelClear = m_SpatialHashingShader.FindKernel("Clear");
		m_KernelBuildHash = m_SpatialHashingShader.FindKernel("BuildHash");
		m_KernelFindPairs = m_SpatialHashingShader.FindKernel("FindPairs");
		m_SpatialHashmapSoA = new GPUSpatialHashmapSoA(PBDMath.NextPrimeNumber(100000));
	}

	public override void Dispose()
	{
		base.Dispose();
		m_SpatialHashmapSoA.Dispose();
	}

	public override void Update(CommandBuffer cmd)
	{
		m_SpatialHashmapSoA.SetGlobalData(cmd);
		m_BodyColliderPairsSoA.SetGlobalData(cmd);
		m_BodyForceVolumePairsSoA.SetGlobalData(cmd);
		cmd.SetGlobalFloat(_InvCellSize, math.rcp(m_Settings.CellSize));
		cmd.SetGlobalFloat(_HashtableCapacity, m_SpatialHashmapSoA.Length);
		cmd.SetComputeIntParam(m_SpatialHashingShader, _BodyColliderPairsCapacity, m_BodyColliderPairsSoA.Length);
		cmd.SetComputeIntParam(m_SpatialHashingShader, _BodyForceVolumePairsCapacity, m_BodyForceVolumePairsSoA.Length);
		cmd.DispatchCompute(m_SpatialHashingShader, m_KernelClear, RenderingUtils.DivRoundUp(m_SpatialHashmapSoA.Length, 64), 1, 1);
		int num = m_CollidersCount + m_ForceVolumesCount + m_BodiesCount;
		if (num > 0)
		{
			cmd.SetComputeIntParam(m_SpatialHashingShader, _AabbCount, num);
			cmd.DispatchCompute(m_SpatialHashingShader, m_KernelBuildHash, RenderingUtils.DivRoundUp(num, 64), 1, 1);
		}
		cmd.SetGlobalInt(_BodiesAabbCount, m_BodiesCount);
		cmd.SetGlobalInt(_BodiesAabbOffset, base.BodiesAabbOffset);
		cmd.SetGlobalInt(_ColliderAabbCount, m_CollidersCount);
		cmd.SetGlobalInt(_ColliderAabbOffset, base.CollidersAabbOffset);
		cmd.SetGlobalInt(_ForceVolumeAabbCount, m_ForceVolumesCount);
		cmd.SetGlobalInt(_ForceVolumeAabbOffset, base.ForceVolumesAabbOffset);
		if (m_BodiesCount > 0)
		{
			cmd.DispatchCompute(m_SpatialHashingShader, m_KernelFindPairs, RenderingUtils.DivRoundUp(m_BodiesCount, 64), 1, 1);
		}
	}
}
