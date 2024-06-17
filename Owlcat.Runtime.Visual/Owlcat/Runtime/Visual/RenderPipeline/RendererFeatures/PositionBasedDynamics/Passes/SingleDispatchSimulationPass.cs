using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Owlcat.Runtime.Core.Utility.Buffers;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Broadphase;
using Owlcat.Runtime.Visual.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Passes;

public class SingleDispatchSimulationPass : ScriptableRenderPass
{
	private const string kProfilerTag = "PBD Single Dispatch Simulation";

	private const int kMaxCamerasCount = 4;

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("PBD Single Dispatch Simulation");

	private ComputeShader m_SimulationShader;

	private int m_KernelSimulateGrass;

	private int m_KernelSimulate32;

	private int m_KernelSimulate64;

	private int m_KernelSimulate128;

	private int m_KernelSimulate192;

	private int m_KernelSimulate256;

	private int m_KernelSimulate512;

	private ComputeShader m_CollisionShader;

	private int m_KernelUpdateColliders;

	private int m_KernelClearCollidersGrid;

	private int m_KernelUpdateCollidersGrid;

	private ComputeShader m_ForceVolumeShader;

	private int m_KernelUpdateForceVolumes;

	private int m_KernelClearForceVolumesGrid;

	private int m_KernelUpdateForceFolumesGrid;

	private ComputeShader m_SkinningShader;

	private int m_KernelSkinnedUpdateParticlesSingleDispatch;

	private int m_KernelSkinnedUpdateBindposesSingleDispatch;

	private ComputeShader m_MeshShader;

	private int m_KernelMeshUpdateParticlesSingleDispatch;

	private int m_KernelMeshUpdateNormalsAndTangentsSingleDispatch;

	private ComputeShader m_BodyAabbShader;

	private int m_KernelUpdateBodyAabb32;

	private int m_KernelUpdateBodyAabb64;

	private int m_KernelUpdateBodyAabb128;

	private int m_KernelUpdateBodyAabb192;

	private int m_KernelUpdateBodyAabb256;

	private int m_KernelUpdateBodyAabb512;

	private ComputeShader m_CameraCullingShader;

	private int m_KernelCameraCull;

	private GPUData m_GpuData;

	private int m_SimulationIterations;

	private int m_ConstraintIterations;

	private float m_Decay;

	private int m_LastFrameId;

	private ArrayPool<Matrix4x4> m_MatrixPool;

	private ArrayPool<Vector4> m_Vector4Pool;

	private Camera[] m_Cameras = new Camera[4];

	private GPUBroadphaseBase m_Broadphase;

	public SingleDispatchSimulationPass(RenderPassEvent evt, ComputeShader simulationShader, ComputeShader collisionShader, ComputeShader forceVolumeShader, ComputeShader skinningShader, ComputeShader meshShader, ComputeShader bodyAabbShader, ComputeShader cameraCullingShader)
	{
		base.RenderPassEvent = evt;
		m_SimulationShader = simulationShader;
		m_KernelSimulateGrass = m_SimulationShader.FindKernel("SimulateGrass");
		m_KernelSimulate32 = m_SimulationShader.FindKernel("Simulate32");
		m_KernelSimulate64 = m_SimulationShader.FindKernel("Simulate64");
		m_KernelSimulate128 = m_SimulationShader.FindKernel("Simulate128");
		m_KernelSimulate192 = m_SimulationShader.FindKernel("Simulate192");
		m_KernelSimulate256 = m_SimulationShader.FindKernel("Simulate256");
		m_KernelSimulate512 = m_SimulationShader.FindKernel("Simulate512");
		m_CollisionShader = collisionShader;
		m_KernelUpdateColliders = m_CollisionShader.FindKernel("UpdateColliders");
		m_KernelClearCollidersGrid = m_CollisionShader.FindKernel("ClearCollidersGrid");
		m_KernelUpdateCollidersGrid = m_CollisionShader.FindKernel("UpdateCollidersGrid");
		m_ForceVolumeShader = forceVolumeShader;
		m_KernelUpdateForceVolumes = m_ForceVolumeShader.FindKernel("UpdateForceVolumes");
		m_KernelClearForceVolumesGrid = m_ForceVolumeShader.FindKernel("ClearForceVolumesGrid");
		m_KernelUpdateForceFolumesGrid = m_ForceVolumeShader.FindKernel("UpdateForceFolumesGrid");
		m_SkinningShader = skinningShader;
		m_KernelSkinnedUpdateParticlesSingleDispatch = m_SkinningShader.FindKernel("UpdateParticlesSingleDispatch");
		m_KernelSkinnedUpdateBindposesSingleDispatch = m_SkinningShader.FindKernel("UpdateBindposesSingleDispatch");
		m_MeshShader = meshShader;
		m_KernelMeshUpdateParticlesSingleDispatch = m_MeshShader.FindKernel("UpdateParticlesSingleDispatch");
		m_KernelMeshUpdateNormalsAndTangentsSingleDispatch = m_MeshShader.FindKernel("UpdateNormalsAndTangentsSingleDispatch");
		m_BodyAabbShader = bodyAabbShader;
		m_KernelUpdateBodyAabb32 = m_BodyAabbShader.FindKernel("UpdateBodyAabb32");
		m_KernelUpdateBodyAabb64 = m_BodyAabbShader.FindKernel("UpdateBodyAabb64");
		m_KernelUpdateBodyAabb128 = m_BodyAabbShader.FindKernel("UpdateBodyAabb128");
		m_KernelUpdateBodyAabb192 = m_BodyAabbShader.FindKernel("UpdateBodyAabb192");
		m_KernelUpdateBodyAabb256 = m_BodyAabbShader.FindKernel("UpdateBodyAabb256");
		m_KernelUpdateBodyAabb512 = m_BodyAabbShader.FindKernel("UpdateBodyAabb512");
		m_CameraCullingShader = cameraCullingShader;
		m_KernelCameraCull = cameraCullingShader.FindKernel("CameraCull");
		m_MatrixPool = ArrayPool<Matrix4x4>.Create();
		m_Vector4Pool = ArrayPool<Vector4>.Create();
	}

	public void Setup(GPUBroadphaseBase broadphase, int simulationIterations, int constraintIterations, float decay)
	{
		m_Broadphase = broadphase;
		m_GpuData = PBD.GetGPUData();
		m_SimulationIterations = simulationIterations;
		m_ConstraintIterations = constraintIterations;
		m_Decay = decay;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (!m_GpuData.IsValid || PBD.IsEmpty)
		{
			return;
		}
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		commandBuffer.SetGlobalFloat(PositionBasedDynamicsConstantBuffer._PbdEnabledGlobal, 1f);
		if (m_LastFrameId != FrameId.FrameCount)
		{
			using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
			{
				if (PBD.WillSimulateOnCurrentFrame || PBD.DebugSettings.UpdateEveryFrame)
				{
					Tick(commandBuffer);
				}
			}
			m_LastFrameId = FrameId.FrameCount;
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	private void Tick(CommandBuffer cmd)
	{
		m_Broadphase.ResizeBuffers(m_GpuData.GlobalCollidersCount, m_GpuData.BodiesCountWithoutGrass, m_GpuData.ForceVolumesCount);
		SetupShaderConstants(cmd);
		UpdateParticles(cmd);
		UpdateColliders(cmd);
		UpdateForceVolumes(cmd);
		UpdateBodyAabb(cmd);
		CameraCulling(cmd);
		m_Broadphase.Update(cmd);
		m_GpuData.BodyGroupsOffsetCount.TryGetValue(BodyGroups.GroupGrass, out var value);
		foreach (KeyValuePair<BodyGroups, int2> item in m_GpuData.BodyGroupsOffsetCount)
		{
			int num = m_KernelSimulate32;
			int x = item.Value.x;
			int num2 = item.Value.y;
			if (num2 > 0)
			{
				switch (item.Key)
				{
				case BodyGroups.GroupGrass:
				{
					num = m_KernelSimulateGrass;
					m_SimulationShader.GetKernelThreadGroupSizes(num, out var x2, out var _, out var _);
					num2 = RenderingUtils.DivRoundUp(m_GpuData.GrassParticlesCount, (int)x2);
					break;
				}
				case BodyGroups.Group32:
					num = m_KernelSimulate32;
					break;
				case BodyGroups.Group64:
					num = m_KernelSimulate64;
					break;
				case BodyGroups.Group128:
					num = m_KernelSimulate128;
					break;
				case BodyGroups.Group192:
					num = m_KernelSimulate192;
					break;
				case BodyGroups.Group256:
					num = m_KernelSimulate256;
					break;
				case BodyGroups.Group512:
					num = m_KernelSimulate512;
					break;
				}
				if (num != m_KernelSimulateGrass)
				{
					m_GpuData.DisconnectedConstraintsGroupsSoA.SetComputeData(cmd, m_SimulationShader, num);
				}
				m_GpuData.BodyDescriptorsSoA.SetComputeData(cmd, m_SimulationShader, num);
				m_GpuData.BodyGroupsIndicesSoA.SetComputeData(cmd, m_SimulationShader, num);
				cmd.SetComputeIntParam(m_SimulationShader, PositionBasedDynamicsConstantBuffer._GrassBodyCount, value.y);
				cmd.SetComputeIntParam(m_SimulationShader, PositionBasedDynamicsConstantBuffer._BodyGroupOffset, x);
				cmd.SetComputeIntParam(m_SimulationShader, PositionBasedDynamicsConstantBuffer._BodiesAabbOffset, m_Broadphase.BodiesAabbOffset);
				cmd.DispatchCompute(m_SimulationShader, num, num2, 1, 1);
			}
		}
		Skinning(cmd);
		Mesh(cmd);
		cmd.SetGlobalBuffer(GPUParticleSoA._PbdParticlesPositionBuffer, m_GpuData.ParticlesSoA.PositionBuffer);
	}

	private void SetupShaderConstants(CommandBuffer cmd)
	{
		cmd.SetComputeFloatParam(m_SimulationShader, PositionBasedDynamicsConstantBuffer._Decay, m_Decay);
		cmd.SetComputeFloatParam(m_SimulationShader, PositionBasedDynamicsConstantBuffer._VelocitySleepThreshold, 1f);
		cmd.SetComputeFloatParam(m_SimulationShader, PositionBasedDynamicsConstantBuffer._DeltaTime, PBD.UpdatePeriod);
		cmd.SetComputeIntParam(m_SimulationShader, PositionBasedDynamicsConstantBuffer._SimulationIterations, m_SimulationIterations);
		cmd.SetComputeIntParam(m_SimulationShader, PositionBasedDynamicsConstantBuffer._ConstraintIterations, m_ConstraintIterations);
		cmd.SetComputeIntParam(m_SimulationShader, PositionBasedDynamicsConstantBuffer._GrassParticlesCount, m_GpuData.GrassParticlesCount);
		cmd.SetComputeIntParam(m_SimulationShader, PositionBasedDynamicsConstantBuffer._GlobalCollidersCount, m_GpuData.GlobalCollidersCount);
		cmd.SetComputeIntParam(m_SimulationShader, PositionBasedDynamicsConstantBuffer._ForceVolumesCount, m_GpuData.ForceVolumes?.Count ?? 0);
		cmd.SetComputeIntParam(m_ForceVolumeShader, PositionBasedDynamicsConstantBuffer._ForceVolumesAabbOffset, m_Broadphase.ForceVolumesAabbOffset);
		m_GpuData.BodyDescriptorsSoA.SetGlobalData(cmd);
		m_GpuData.ParticlesSoA.SetGlobalData(cmd);
		m_GpuData.ConstraintsSoA.SetGlobalData(cmd);
		if (m_GpuData.SkinnedBodyDescriptorsMap.Count > 0)
		{
			m_GpuData.SkinnedBodySoA.SetGlobalData(cmd);
		}
		if (m_GpuData.MeshBodyDescriptorsMap.Count > 0)
		{
			m_GpuData.MeshBodyIndicesSoA.SetGlobalData(cmd);
			m_GpuData.MeshBodyVerticesSoA.SetGlobalData(cmd);
			m_GpuData.MeshBodyVertexTriangleMapSoA.SetGlobalData(cmd);
			m_GpuData.MeshBodyVertexTriangleMapOffsetCountSoA.SetGlobalData(cmd);
			m_GpuData.BodyWorldToLocalMatricesSoA.SetGlobalData(cmd);
		}
		if (m_GpuData.CollidersSoA != null)
		{
			m_GpuData.CollidersSoA.SetGlobalData(cmd);
		}
		if (m_GpuData.ForceVolumesSoA != null)
		{
			m_GpuData.ForceVolumesSoA.SetGlobalData(cmd);
		}
		cmd.SetComputeFloatParam(m_SimulationShader, PositionBasedDynamicsConstantBuffer._GlobalWindEnabled, 0f);
		foreach (IForce force in m_GpuData.Forces)
		{
			if (force.Body == null)
			{
				force.SetupShader(m_SimulationShader, cmd);
			}
		}
		m_Broadphase.SceneAabbSoA.SetGlobalData(cmd);
		m_Broadphase.BoundingBoxSoA.SetGlobalData(cmd);
		cmd.SetGlobalInt(PositionBasedDynamicsConstantBuffer._BroadphaseType, (int)m_Broadphase.Type);
	}

	private void UpdateParticles(CommandBuffer cmd)
	{
		if (m_GpuData.SkinnedBodyDescriptorsMap.Count > 0)
		{
			m_GpuData.SkinnedBodyDescriptorsIndicesSoA.SetComputeData(cmd, m_SkinningShader, m_KernelSkinnedUpdateParticlesSingleDispatch);
			int num = 0;
			int num2 = 0;
			Matrix4x4[] array = m_MatrixPool.Rent(256);
			foreach (KeyValuePair<SkinnedBody, int> item in m_GpuData.SkinnedBodyDescriptorsMap)
			{
				array[num] = item.Key.LocalToWorld;
				num++;
				if (num >= 256 || num + num2 >= m_GpuData.SkinnedBodyDescriptorsMap.Count)
				{
					cmd.SetComputeIntParam(m_SkinningShader, PositionBasedDynamicsConstantBuffer._Offset, num2);
					cmd.SetComputeMatrixArrayParam(m_SkinningShader, PositionBasedDynamicsConstantBuffer._WorldMatrices, array);
					cmd.DispatchCompute(m_SkinningShader, m_KernelSkinnedUpdateParticlesSingleDispatch, num, 1, 1);
					num2 += 256;
					num = 0;
				}
			}
			m_MatrixPool.Return(array);
		}
		if (m_GpuData.MeshBodyDescriptorsMap.Count <= 0)
		{
			return;
		}
		m_GpuData.MeshBodyDescriptorsIndicesSoA.SetComputeData(cmd, m_MeshShader, m_KernelMeshUpdateParticlesSingleDispatch);
		m_GpuData.BodyWorldToLocalMatricesSoA.SetComputeData(cmd, m_MeshShader, m_KernelMeshUpdateParticlesSingleDispatch);
		int num3 = 0;
		int num4 = 0;
		Matrix4x4[] array2 = m_MatrixPool.Rent(256);
		Matrix4x4[] array3 = m_MatrixPool.Rent(256);
		foreach (KeyValuePair<MeshBody, int> item2 in m_GpuData.MeshBodyDescriptorsMap)
		{
			array2[num3] = item2.Key.LocalToWorld;
			array3[num3] = item2.Key.LocalToWorld.inverse;
			num3++;
			if (num3 >= 256 || num3 + num4 >= m_GpuData.MeshBodyDescriptorsMap.Count)
			{
				cmd.SetComputeIntParam(m_MeshShader, PositionBasedDynamicsConstantBuffer._Offset, num4);
				cmd.SetComputeMatrixArrayParam(m_MeshShader, PositionBasedDynamicsConstantBuffer._WorldMatrices, array2);
				cmd.SetComputeMatrixArrayParam(m_MeshShader, PositionBasedDynamicsConstantBuffer._InvWorldMatrices, array3);
				cmd.DispatchCompute(m_MeshShader, m_KernelMeshUpdateParticlesSingleDispatch, num3, 1, 1);
				num4 += 256;
				num3 = 0;
			}
		}
		m_MatrixPool.Return(array2);
		m_MatrixPool.Return(array3);
	}

	private void UpdateColliders(CommandBuffer cmd)
	{
		if (m_GpuData.ColliderRefs == null || m_GpuData.ColliderRefs.Count == 0 || m_GpuData.ColliderRefs.Count <= 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		Matrix4x4[] array = m_MatrixPool.Rent(256);
		Vector4[] array2 = m_Vector4Pool.Rent(256);
		Vector4[] array3 = m_Vector4Pool.Rent(256);
		foreach (ColliderRef colliderRef in m_GpuData.ColliderRefs)
		{
			array[num] = colliderRef.World;
			array2[num] = colliderRef.Parameters0;
			array3[num] = new Vector4(colliderRef.Restitution, colliderRef.Friction, (float)colliderRef.Type, 0f);
			num++;
			if (num >= 256 || num >= m_GpuData.ColliderRefs.Count)
			{
				cmd.SetComputeMatrixArrayParam(m_CollisionShader, PositionBasedDynamicsConstantBuffer._Matrices, array);
				cmd.SetComputeVectorArrayParam(m_CollisionShader, PositionBasedDynamicsConstantBuffer._Parameters0, array2);
				cmd.SetComputeVectorArrayParam(m_CollisionShader, PositionBasedDynamicsConstantBuffer._MaterialParameters, array3);
				cmd.SetComputeIntParam(m_CollisionShader, PositionBasedDynamicsConstantBuffer._Offset, num2);
				cmd.SetComputeIntParam(m_CollisionShader, PositionBasedDynamicsConstantBuffer._Count, num);
				cmd.DispatchCompute(m_CollisionShader, m_KernelUpdateColliders, RenderingUtils.DivRoundUp(num, 64), 1, 1);
				num2 += 256;
				num = 0;
			}
		}
		m_MatrixPool.Return(array);
		m_Vector4Pool.Return(array2);
		m_Vector4Pool.Return(array3);
	}

	private void UpdateForceVolumes(CommandBuffer cmd)
	{
		if (m_GpuData.ForceVolumes == null || m_GpuData.ForceVolumes.Count == 0 || m_GpuData.ForceVolumes.Count <= 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		Matrix4x4[] array = m_MatrixPool.Rent(256);
		Matrix4x4[] array2 = m_MatrixPool.Rent(256);
		Vector4[] array3 = m_Vector4Pool.Rent(256);
		Vector4[] array4 = m_Vector4Pool.Rent(256);
		Vector4[] array5 = m_Vector4Pool.Rent(256);
		foreach (ForceVolume forceVolume in m_GpuData.ForceVolumes)
		{
			int packedEnumValues = ForceVolume.GetPackedEnumValues(forceVolume);
			array[num] = forceVolume.LocalToWorldVolume;
			array3[num] = forceVolume.VolumeParameters;
			array2[num] = forceVolume.LocalToWorldEmitter;
			array5[num] = (Vector3)forceVolume.Direction;
			array4[num] = new Vector4(packedEnumValues, forceVolume.DirectionLerp, forceVolume.Intensity, 0f);
			num++;
			if (num >= 256 || num >= m_GpuData.ForceVolumes.Count)
			{
				cmd.SetComputeMatrixArrayParam(m_ForceVolumeShader, PositionBasedDynamicsConstantBuffer._LocalToWorldVolumeMatrices, array);
				cmd.SetComputeMatrixArrayParam(m_ForceVolumeShader, PositionBasedDynamicsConstantBuffer._LocalToWorldEmitterMatrices, array2);
				cmd.SetComputeVectorArrayParam(m_ForceVolumeShader, PositionBasedDynamicsConstantBuffer._VolumeParameters0, array3);
				cmd.SetComputeVectorArrayParam(m_ForceVolumeShader, PositionBasedDynamicsConstantBuffer._VolumeParameters1, array4);
				cmd.SetComputeVectorArrayParam(m_ForceVolumeShader, PositionBasedDynamicsConstantBuffer._EmitterDirection, array5);
				cmd.SetComputeIntParam(m_ForceVolumeShader, PositionBasedDynamicsConstantBuffer._Offset, num2);
				cmd.SetComputeIntParam(m_ForceVolumeShader, PositionBasedDynamicsConstantBuffer._Count, num);
				cmd.DispatchCompute(m_ForceVolumeShader, m_KernelUpdateForceVolumes, RenderingUtils.DivRoundUp(num, 64), 1, 1);
				num2 += 256;
				num = 0;
			}
		}
		m_MatrixPool.Return(array);
		m_MatrixPool.Return(array2);
		m_Vector4Pool.Return(array3);
		m_Vector4Pool.Return(array4);
		m_Vector4Pool.Return(array5);
	}

	private void UpdateBodyAabb(CommandBuffer cmd)
	{
		m_GpuData.BodyGroupsOffsetCount.TryGetValue(BodyGroups.GroupGrass, out var value);
		foreach (KeyValuePair<BodyGroups, int2> item in m_GpuData.BodyGroupsOffsetCount)
		{
			int num = m_KernelUpdateBodyAabb32;
			int x = item.Value.x;
			int y = item.Value.y;
			if (y <= 0)
			{
				continue;
			}
			BodyGroups key = item.Key;
			if (key <= BodyGroups.Group64)
			{
				if (key == BodyGroups.GroupGrass)
				{
					continue;
				}
				switch (key)
				{
				case BodyGroups.Group32:
					num = m_KernelUpdateBodyAabb32;
					break;
				case BodyGroups.Group64:
					num = m_KernelUpdateBodyAabb64;
					break;
				}
			}
			else
			{
				switch (key)
				{
				case BodyGroups.Group128:
					num = m_KernelUpdateBodyAabb128;
					break;
				case BodyGroups.Group192:
					num = m_KernelUpdateBodyAabb192;
					break;
				case BodyGroups.Group256:
					num = m_KernelUpdateBodyAabb256;
					break;
				case BodyGroups.Group512:
					num = m_KernelUpdateBodyAabb512;
					break;
				}
			}
			m_GpuData.BodyDescriptorsSoA.SetComputeData(cmd, m_BodyAabbShader, num);
			m_GpuData.BodyGroupsIndicesSoA.SetComputeData(cmd, m_BodyAabbShader, num);
			cmd.SetComputeIntParam(m_BodyAabbShader, PositionBasedDynamicsConstantBuffer._BodyGroupOffset, x);
			cmd.SetComputeIntParam(m_BodyAabbShader, PositionBasedDynamicsConstantBuffer._GrassBodyCount, value.y);
			cmd.SetComputeIntParam(m_BodyAabbShader, PositionBasedDynamicsConstantBuffer._BodiesAabbOffset, m_Broadphase.BodiesAabbOffset);
			cmd.DispatchCompute(m_BodyAabbShader, num, y, 1, 1);
		}
	}

	private void CameraCulling(CommandBuffer cmd)
	{
		if (m_GpuData.BodiesCountWithoutGrass <= 0)
		{
			return;
		}
		if (m_GpuData.BodyVisibilitySoA.Length < m_GpuData.BodiesCountWithoutGrass)
		{
			m_GpuData.BodyVisibilitySoA.Resize((int)((float)m_GpuData.BodiesCountWithoutGrass * 1.5f));
		}
		Matrix4x4[] array = m_MatrixPool.Rent(4);
		int num = 0;
		Camera.GetAllCameras(m_Cameras);
		Camera[] cameras = m_Cameras;
		foreach (Camera camera in cameras)
		{
			if (!(camera == null) && camera.enabled)
			{
				if (camera.cameraType == CameraType.Game || camera.cameraType == CameraType.SceneView)
				{
					array[num] = camera.projectionMatrix * camera.worldToCameraMatrix;
					num++;
				}
				if (num >= 4)
				{
					break;
				}
			}
		}
		m_GpuData.BodyVisibilitySoA.SetGlobalData(cmd);
		cmd.SetComputeMatrixArrayParam(m_CameraCullingShader, PositionBasedDynamicsConstantBuffer._ViewProj, array);
		cmd.SetComputeIntParam(m_CameraCullingShader, PositionBasedDynamicsConstantBuffer._CamerasCount, num);
		cmd.SetComputeIntParam(m_CameraCullingShader, PositionBasedDynamicsConstantBuffer._Count, m_GpuData.BodiesCountWithoutGrass);
		cmd.SetComputeIntParam(m_CameraCullingShader, PositionBasedDynamicsConstantBuffer._BodiesAabbOffset, m_Broadphase.BodiesAabbOffset);
		cmd.DispatchCompute(m_CameraCullingShader, m_KernelCameraCull, RenderingUtils.DivRoundUp(m_GpuData.BodiesCountWithoutGrass, 64), 1, 1);
		m_MatrixPool.Return(array);
	}

	private void Skinning(CommandBuffer cmd)
	{
		if (m_GpuData.SkinnedBodyDescriptorsMap.Count <= 0)
		{
			return;
		}
		m_GpuData.SkinnedBodyDescriptorsIndicesSoA.SetComputeData(cmd, m_SkinningShader, m_KernelSkinnedUpdateBindposesSingleDispatch);
		int num = 0;
		int num2 = 0;
		Matrix4x4[] array = m_MatrixPool.Rent(256);
		Matrix4x4[] array2 = m_MatrixPool.Rent(256);
		foreach (KeyValuePair<SkinnedBody, int> item in m_GpuData.SkinnedBodyDescriptorsMap)
		{
			array[num] = item.Key.LocalToWorld;
			array2[num] = item.Key.LocalToWorld.inverse;
			num++;
			if (num >= 256 || num + num2 >= m_GpuData.SkinnedBodyDescriptorsMap.Count)
			{
				cmd.SetComputeIntParam(m_SkinningShader, PositionBasedDynamicsConstantBuffer._Offset, num2);
				cmd.SetComputeMatrixArrayParam(m_SkinningShader, PositionBasedDynamicsConstantBuffer._WorldMatrices, array);
				cmd.SetComputeMatrixArrayParam(m_SkinningShader, PositionBasedDynamicsConstantBuffer._InvWorldMatrices, array2);
				cmd.DispatchCompute(m_SkinningShader, m_KernelSkinnedUpdateBindposesSingleDispatch, num, 1, 1);
				num2 += 256;
				num = 0;
			}
		}
		cmd.SetGlobalBuffer(PositionBasedDynamicsConstantBuffer._PbdBindposes, m_GpuData.SkinnedBodySoA.SimulatedBindposesBuffer);
		cmd.SetGlobalBuffer(PositionBasedDynamicsConstantBuffer._PbdSkinnedBodyBoneIndicesMap, m_GpuData.SkinnedBodyBoneIndicesMapSoA.Buffer);
		cmd.SetGlobalFloat(PositionBasedDynamicsConstantBuffer._PbdEnabledGlobal, 1f);
		m_MatrixPool.Return(array);
		m_MatrixPool.Return(array2);
	}

	private void Mesh(CommandBuffer cmd)
	{
		if (m_GpuData.MeshBodyDescriptorsMap.Count <= 0)
		{
			return;
		}
		m_GpuData.MeshBodyDescriptorsIndicesSoA.SetComputeData(cmd, m_MeshShader, m_KernelMeshUpdateNormalsAndTangentsSingleDispatch);
		int num = 0;
		int num2 = 0;
		Matrix4x4[] array = m_MatrixPool.Rent(256);
		foreach (KeyValuePair<MeshBody, int> item in m_GpuData.MeshBodyDescriptorsMap)
		{
			array[num] = item.Key.LocalToWorld.inverse;
			num++;
			if (num >= 256 || num + num2 >= m_GpuData.MeshBodyDescriptorsMap.Count)
			{
				cmd.SetComputeIntParam(m_MeshShader, PositionBasedDynamicsConstantBuffer._Offset, num2);
				cmd.SetComputeMatrixArrayParam(m_MeshShader, PositionBasedDynamicsConstantBuffer._InvWorldMatrices, array);
				cmd.DispatchCompute(m_MeshShader, m_KernelMeshUpdateNormalsAndTangentsSingleDispatch, num, 1, 1);
				num2 += num;
				num = 0;
			}
		}
		cmd.SetGlobalBuffer(PositionBasedDynamicsConstantBuffer._PBDNormals, m_GpuData.MeshBodyVerticesSoA.NormalsBuffer);
		cmd.SetGlobalBuffer(PositionBasedDynamicsConstantBuffer._PBDTangents, m_GpuData.MeshBodyVerticesSoA.TangentsBuffer);
		cmd.SetGlobalFloat(PositionBasedDynamicsConstantBuffer._PbdEnabledGlobal, 1f);
		m_MatrixPool.Return(array);
	}
}
