using System.Buffers;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics.Broadphase;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics.Passes;

public class SimulationPass : ScriptableRenderPass<SimulationPassData>
{
	private const int kMaxCamerasCount = 4;

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

	private PositionBasedDynamicsConfig m_Config;

	private Camera[] m_Cameras = new Camera[4];

	private GPUBroadphaseBase m_Broadphase;

	private ArrayPool<Matrix4x4> m_MatrixPool;

	private ArrayPool<Vector4> m_Vector4Pool;

	public override string Name => "SimulationPass";

	public SimulationPass(RenderPassEvent evt, ComputeShader simulationShader, ComputeShader collisionShader, ComputeShader forceVolumeShader, ComputeShader skinningShader, ComputeShader meshShader, ComputeShader bodyAabbShader, ComputeShader cameraCullingShader)
		: base(evt)
	{
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
		m_Config = PositionBasedDynamicsConfig.Instance;
	}

	internal void Init(GPUBroadphaseBase broadphase, int simulationIterations, int constraintIterations, float decay)
	{
		m_Broadphase = broadphase;
		m_GpuData = PBD.GetGPUData();
		m_SimulationIterations = simulationIterations;
		m_ConstraintIterations = constraintIterations;
		m_Decay = decay;
	}

	protected override void Setup(RenderGraphBuilder builder, SimulationPassData data, ref RenderingData renderingData)
	{
		data.SimulationShader = m_SimulationShader;
		data.KernelSimulateGrass = m_KernelSimulateGrass;
		data.KernelSimulate32 = m_KernelSimulate32;
		data.KernelSimulate64 = m_KernelSimulate64;
		data.KernelSimulate128 = m_KernelSimulate128;
		data.KernelSimulate192 = m_KernelSimulate192;
		data.KernelSimulate256 = m_KernelSimulate256;
		data.KernelSimulate512 = m_KernelSimulate512;
		data.CollisionShader = m_CollisionShader;
		data.KernelUpdateColliders = m_KernelUpdateColliders;
		data.KernelClearCollidersGrid = m_KernelClearCollidersGrid;
		data.KernelUpdateCollidersGrid = m_KernelUpdateCollidersGrid;
		data.ForceVolumeShader = m_ForceVolumeShader;
		data.KernelUpdateForceVolumes = m_KernelUpdateForceVolumes;
		data.KernelClearForceVolumesGrid = m_KernelClearForceVolumesGrid;
		data.KernelUpdateForceFolumesGrid = m_KernelUpdateForceFolumesGrid;
		data.SkinningShader = m_SkinningShader;
		data.KernelSkinnedUpdateParticlesSingleDispatch = m_KernelSkinnedUpdateParticlesSingleDispatch;
		data.KernelSkinnedUpdateBindposesSingleDispatch = m_KernelSkinnedUpdateBindposesSingleDispatch;
		data.MeshShader = m_MeshShader;
		data.KernelMeshUpdateParticlesSingleDispatch = m_KernelMeshUpdateParticlesSingleDispatch;
		data.KernelMeshUpdateNormalsAndTangentsSingleDispatch = m_KernelMeshUpdateNormalsAndTangentsSingleDispatch;
		data.BodyAabbShader = m_BodyAabbShader;
		data.KernelUpdateBodyAabb32 = m_KernelUpdateBodyAabb32;
		data.KernelUpdateBodyAabb64 = m_KernelUpdateBodyAabb64;
		data.KernelUpdateBodyAabb128 = m_KernelUpdateBodyAabb128;
		data.KernelUpdateBodyAabb192 = m_KernelUpdateBodyAabb192;
		data.KernelUpdateBodyAabb256 = m_KernelUpdateBodyAabb256;
		data.KernelUpdateBodyAabb512 = m_KernelUpdateBodyAabb512;
		data.CameraCullingShader = m_CameraCullingShader;
		data.KernelCameraCull = m_KernelCameraCull;
		data.Broadphase = m_Broadphase;
		data.GpuData = m_GpuData;
		data.SimulationIterations = m_SimulationIterations;
		data.ConstraintIterations = m_ConstraintIterations;
		data.Decay = m_Decay;
		data.Cameras = m_Cameras;
		data.MatrixPool = m_MatrixPool;
		data.Vector4Pool = m_Vector4Pool;
		data.CameraCullingEnabled = m_Config.CameraCullingEnabled;
		data.Simulate = false;
		if (m_LastFrameId != renderingData.TimeData.FrameId)
		{
			if (PBD.WillSimulateOnCurrentFrame || PBD.DebugSettings.UpdateEveryFrame)
			{
				data.Simulate = true;
			}
			m_LastFrameId = renderingData.TimeData.FrameId;
		}
	}

	protected override void Render(SimulationPassData data, RenderGraphContext context)
	{
		if (data.GpuData.IsValid && !PBD.IsEmpty)
		{
			context.cmd.SetGlobalFloat(PositionBasedDynamicsConstantBuffer._PbdEnabledGlobal, 1f);
			if (data.Simulate)
			{
				Tick(data, context);
			}
		}
	}

	private void Tick(SimulationPassData data, RenderGraphContext context)
	{
		data.Broadphase.ResizeBuffers(data.GpuData.GlobalCollidersCount, data.GpuData.BodiesCountWithoutGrass, data.GpuData.ForceVolumesCount);
		SetupShaderConstants(data, context);
		UpdateParticles(data, context);
		UpdateColliders(data, context);
		UpdateForceVolumes(data, context);
		UpdateBodyAabb(data, context);
		CameraCulling(data, context);
		data.Broadphase.Update(context.cmd);
		SimulationStep(data, context);
		Skinning(data, context);
		Mesh(data, context);
		context.cmd.SetGlobalBuffer(GPUParticleSoA._PbdParticlesPositionPairsBuffer, data.GpuData.ParticlesSoA.PositionPairsBuffer);
	}

	private void SetupShaderConstants(SimulationPassData data, RenderGraphContext context)
	{
		context.cmd.SetComputeFloatParam(data.SimulationShader, PositionBasedDynamicsConstantBuffer._Decay, data.Decay);
		context.cmd.SetComputeFloatParam(data.SimulationShader, PositionBasedDynamicsConstantBuffer._VelocitySleepThreshold, 1f);
		context.cmd.SetComputeFloatParam(data.SimulationShader, PositionBasedDynamicsConstantBuffer._DeltaTime, PBD.UpdatePeriod);
		context.cmd.SetComputeIntParam(data.SimulationShader, PositionBasedDynamicsConstantBuffer._SimulationIterations, data.SimulationIterations);
		context.cmd.SetComputeIntParam(data.SimulationShader, PositionBasedDynamicsConstantBuffer._ConstraintIterations, data.ConstraintIterations);
		context.cmd.SetComputeIntParam(data.SimulationShader, PositionBasedDynamicsConstantBuffer._GrassParticlesCount, data.GpuData.GrassParticlesCount);
		context.cmd.SetComputeIntParam(data.SimulationShader, PositionBasedDynamicsConstantBuffer._GlobalCollidersCount, data.GpuData.GlobalCollidersCount);
		context.cmd.SetComputeIntParam(data.SimulationShader, PositionBasedDynamicsConstantBuffer._ForceVolumesCount, data.GpuData.ForceVolumes?.Count ?? 0);
		context.cmd.SetComputeIntParam(data.ForceVolumeShader, PositionBasedDynamicsConstantBuffer._ForceVolumesAabbOffset, data.Broadphase.ForceVolumesAabbOffset);
		context.cmd.SetComputeIntParam(data.SimulationShader, PositionBasedDynamicsConstantBuffer._CameraCullingEnabled, data.CameraCullingEnabled ? 1 : 0);
		data.GpuData.BodyDescriptorsSoA.SetGlobalData(context.cmd);
		data.GpuData.ParticlesSoA.SetGlobalData(context.cmd);
		data.GpuData.ConstraintsSoA.SetGlobalData(context.cmd);
		if (data.GpuData.SkinnedBodyDescriptorsMap.Count > 0)
		{
			data.GpuData.SkinnedBodySoA.SetGlobalData(context.cmd);
		}
		if (data.GpuData.MeshBodyDescriptorsMap.Count > 0)
		{
			data.GpuData.MeshBodyIndicesSoA.SetGlobalData(context.cmd);
			data.GpuData.MeshBodyVerticesSoA.SetGlobalData(context.cmd);
			data.GpuData.MeshBodyVertexTriangleMapSoA.SetGlobalData(context.cmd);
			data.GpuData.MeshBodyVertexTriangleMapOffsetCountSoA.SetGlobalData(context.cmd);
			data.GpuData.BodyWorldToLocalMatricesSoA.SetGlobalData(context.cmd);
		}
		if (data.GpuData.CollidersSoA != null)
		{
			data.GpuData.CollidersSoA.SetGlobalData(context.cmd);
		}
		if (data.GpuData.ForceVolumesSoA != null)
		{
			data.GpuData.ForceVolumesSoA.SetGlobalData(context.cmd);
		}
		context.cmd.SetComputeFloatParam(data.SimulationShader, PositionBasedDynamicsConstantBuffer._GlobalWindEnabled, 0f);
		foreach (IForce force in data.GpuData.Forces)
		{
			if (force.Body == null)
			{
				force.SetupShader(data.SimulationShader, context.cmd);
			}
		}
		data.Broadphase.SceneAabbSoA.SetGlobalData(context.cmd);
		data.Broadphase.BoundingBoxSoA.SetGlobalData(context.cmd);
		context.cmd.SetGlobalInt(PositionBasedDynamicsConstantBuffer._BroadphaseType, (int)data.Broadphase.Type);
	}

	private void UpdateParticles(SimulationPassData data, RenderGraphContext context)
	{
		if (data.GpuData.SkinnedBodyDescriptorsMap.Count > 0)
		{
			data.GpuData.SkinnedBodyDescriptorsIndicesSoA.SetComputeData(context.cmd, data.SkinningShader, data.KernelSkinnedUpdateParticlesSingleDispatch);
			int num = 0;
			int num2 = 0;
			Matrix4x4[] array = data.MatrixPool.Rent(256);
			foreach (KeyValuePair<SkinnedBody, int> item in data.GpuData.SkinnedBodyDescriptorsMap)
			{
				array[num] = item.Key.LocalToWorld;
				num++;
				if (num >= 256 || num + num2 >= data.GpuData.SkinnedBodyDescriptorsMap.Count)
				{
					context.cmd.SetComputeIntParam(data.SkinningShader, PositionBasedDynamicsConstantBuffer._Offset, num2);
					context.cmd.SetComputeMatrixArrayParam(data.SkinningShader, PositionBasedDynamicsConstantBuffer._WorldMatrices, array);
					context.cmd.DispatchCompute(data.SkinningShader, data.KernelSkinnedUpdateParticlesSingleDispatch, num, 1, 1);
					num2 += 256;
					num = 0;
				}
			}
			data.MatrixPool.Return(array);
		}
		if (data.GpuData.MeshBodyDescriptorsMap.Count <= 0)
		{
			return;
		}
		data.GpuData.MeshBodyDescriptorsIndicesSoA.SetComputeData(context.cmd, data.MeshShader, data.KernelMeshUpdateParticlesSingleDispatch);
		data.GpuData.BodyWorldToLocalMatricesSoA.SetComputeData(context.cmd, data.MeshShader, data.KernelMeshUpdateParticlesSingleDispatch);
		int num3 = 0;
		int num4 = 0;
		Matrix4x4[] array2 = data.MatrixPool.Rent(256);
		Matrix4x4[] array3 = data.MatrixPool.Rent(256);
		foreach (KeyValuePair<MeshBody, int> item2 in data.GpuData.MeshBodyDescriptorsMap)
		{
			array2[num3] = item2.Key.LocalToWorld;
			array3[num3] = item2.Key.LocalToWorld.inverse;
			num3++;
			if (num3 >= 256 || num3 + num4 >= data.GpuData.MeshBodyDescriptorsMap.Count)
			{
				context.cmd.SetComputeIntParam(data.MeshShader, PositionBasedDynamicsConstantBuffer._Offset, num4);
				context.cmd.SetComputeMatrixArrayParam(data.MeshShader, PositionBasedDynamicsConstantBuffer._WorldMatrices, array2);
				context.cmd.SetComputeMatrixArrayParam(data.MeshShader, PositionBasedDynamicsConstantBuffer._InvWorldMatrices, array3);
				context.cmd.DispatchCompute(data.MeshShader, data.KernelMeshUpdateParticlesSingleDispatch, num3, 1, 1);
				num4 += 256;
				num3 = 0;
			}
		}
		data.MatrixPool.Return(array2);
		data.MatrixPool.Return(array3);
	}

	private void UpdateColliders(SimulationPassData data, RenderGraphContext context)
	{
		if (data.GpuData.ColliderRefs == null || data.GpuData.ColliderRefs.Count == 0 || data.GpuData.ColliderRefs.Count <= 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		Matrix4x4[] array = data.MatrixPool.Rent(256);
		Vector4[] array2 = data.Vector4Pool.Rent(256);
		Vector4[] array3 = data.Vector4Pool.Rent(256);
		foreach (ColliderRef colliderRef in data.GpuData.ColliderRefs)
		{
			array[num] = colliderRef.World;
			array2[num] = colliderRef.Parameters0;
			array3[num] = new Vector4(colliderRef.Restitution, colliderRef.Friction, (float)colliderRef.Type, 0f);
			num++;
			if (num >= 256 || num >= data.GpuData.ColliderRefs.Count)
			{
				context.cmd.SetComputeMatrixArrayParam(data.CollisionShader, PositionBasedDynamicsConstantBuffer._Matrices, array);
				context.cmd.SetComputeVectorArrayParam(data.CollisionShader, PositionBasedDynamicsConstantBuffer._Parameters0, array2);
				context.cmd.SetComputeVectorArrayParam(data.CollisionShader, PositionBasedDynamicsConstantBuffer._MaterialParameters, array3);
				context.cmd.SetComputeIntParam(data.CollisionShader, PositionBasedDynamicsConstantBuffer._Offset, num2);
				context.cmd.SetComputeIntParam(data.CollisionShader, PositionBasedDynamicsConstantBuffer._Count, num);
				context.cmd.DispatchCompute(data.CollisionShader, data.KernelUpdateColliders, RenderingUtils.DivRoundUp(num, 64), 1, 1);
				num2 += 256;
				num = 0;
			}
		}
		data.MatrixPool.Return(array);
		data.Vector4Pool.Return(array2);
		data.Vector4Pool.Return(array3);
	}

	private void UpdateForceVolumes(SimulationPassData data, RenderGraphContext context)
	{
		if (data.GpuData.ForceVolumes == null || data.GpuData.ForceVolumes.Count == 0 || data.GpuData.ForceVolumes.Count <= 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		Matrix4x4[] array = data.MatrixPool.Rent(256);
		Matrix4x4[] array2 = data.MatrixPool.Rent(256);
		Vector4[] array3 = data.Vector4Pool.Rent(256);
		Vector4[] array4 = data.Vector4Pool.Rent(256);
		Vector4[] array5 = data.Vector4Pool.Rent(256);
		foreach (ForceVolume forceVolume in data.GpuData.ForceVolumes)
		{
			int packedEnumValues = ForceVolume.GetPackedEnumValues(forceVolume);
			array[num] = forceVolume.LocalToWorldVolume;
			array3[num] = forceVolume.VolumeParameters;
			array2[num] = forceVolume.LocalToWorldEmitter;
			array5[num] = (Vector3)forceVolume.Direction;
			array4[num] = new Vector4(packedEnumValues, forceVolume.DirectionLerp, forceVolume.Intensity, 0f);
			num++;
			if (num >= 256 || num >= data.GpuData.ForceVolumes.Count)
			{
				context.cmd.SetComputeMatrixArrayParam(data.ForceVolumeShader, PositionBasedDynamicsConstantBuffer._LocalToWorldVolumeMatrices, array);
				context.cmd.SetComputeMatrixArrayParam(data.ForceVolumeShader, PositionBasedDynamicsConstantBuffer._LocalToWorldEmitterMatrices, array2);
				context.cmd.SetComputeVectorArrayParam(data.ForceVolumeShader, PositionBasedDynamicsConstantBuffer._VolumeParameters0, array3);
				context.cmd.SetComputeVectorArrayParam(data.ForceVolumeShader, PositionBasedDynamicsConstantBuffer._VolumeParameters1, array4);
				context.cmd.SetComputeVectorArrayParam(data.ForceVolumeShader, PositionBasedDynamicsConstantBuffer._EmitterDirection, array5);
				context.cmd.SetComputeIntParam(data.ForceVolumeShader, PositionBasedDynamicsConstantBuffer._Offset, num2);
				context.cmd.SetComputeIntParam(data.ForceVolumeShader, PositionBasedDynamicsConstantBuffer._Count, num);
				context.cmd.DispatchCompute(data.ForceVolumeShader, data.KernelUpdateForceVolumes, RenderingUtils.DivRoundUp(num, 64), 1, 1);
				num2 += 256;
				num = 0;
			}
		}
		data.MatrixPool.Return(array);
		data.MatrixPool.Return(array2);
		data.Vector4Pool.Return(array3);
		data.Vector4Pool.Return(array4);
		data.Vector4Pool.Return(array5);
	}

	private void UpdateBodyAabb(SimulationPassData data, RenderGraphContext context)
	{
		data.GpuData.BodyGroupsOffsetCount.TryGetValue(BodyGroups.GroupGrass, out var value);
		foreach (KeyValuePair<BodyGroups, int2> item in data.GpuData.BodyGroupsOffsetCount)
		{
			int num = data.KernelUpdateBodyAabb32;
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
					num = data.KernelUpdateBodyAabb32;
					break;
				case BodyGroups.Group64:
					num = data.KernelUpdateBodyAabb64;
					break;
				}
			}
			else
			{
				switch (key)
				{
				case BodyGroups.Group128:
					num = data.KernelUpdateBodyAabb128;
					break;
				case BodyGroups.Group192:
					num = data.KernelUpdateBodyAabb192;
					break;
				case BodyGroups.Group256:
					num = data.KernelUpdateBodyAabb256;
					break;
				case BodyGroups.Group512:
					num = data.KernelUpdateBodyAabb512;
					break;
				}
			}
			data.GpuData.BodyDescriptorsSoA.SetComputeData(context.cmd, data.BodyAabbShader, num);
			data.GpuData.BodyGroupsIndicesSoA.SetComputeData(context.cmd, data.BodyAabbShader, num);
			context.cmd.SetComputeIntParam(data.BodyAabbShader, PositionBasedDynamicsConstantBuffer._BodyGroupOffset, x);
			context.cmd.SetComputeIntParam(data.BodyAabbShader, PositionBasedDynamicsConstantBuffer._GrassBodyCount, value.y);
			context.cmd.SetComputeIntParam(data.BodyAabbShader, PositionBasedDynamicsConstantBuffer._BodiesAabbOffset, data.Broadphase.BodiesAabbOffset);
			context.cmd.DispatchCompute(data.BodyAabbShader, num, y, 1, 1);
		}
	}

	private void CameraCulling(SimulationPassData data, RenderGraphContext context)
	{
		if (!m_Config.CameraCullingEnabled || data.GpuData.BodiesCountWithoutGrass <= 0)
		{
			return;
		}
		if (data.GpuData.BodyVisibilitySoA.Length < data.GpuData.BodiesCountWithoutGrass)
		{
			data.GpuData.BodyVisibilitySoA.Resize((int)((float)data.GpuData.BodiesCountWithoutGrass * 1.5f));
		}
		Matrix4x4[] array = data.MatrixPool.Rent(4);
		int num = 0;
		Camera.GetAllCameras(data.Cameras);
		Camera[] cameras = data.Cameras;
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
		data.GpuData.BodyVisibilitySoA.SetGlobalData(context.cmd);
		context.cmd.SetComputeMatrixArrayParam(data.CameraCullingShader, PositionBasedDynamicsConstantBuffer._ViewProj, array);
		context.cmd.SetComputeIntParam(data.CameraCullingShader, PositionBasedDynamicsConstantBuffer._CamerasCount, num);
		context.cmd.SetComputeIntParam(data.CameraCullingShader, PositionBasedDynamicsConstantBuffer._Count, data.GpuData.BodiesCountWithoutGrass);
		context.cmd.SetComputeIntParam(data.CameraCullingShader, PositionBasedDynamicsConstantBuffer._BodiesAabbOffset, data.Broadphase.BodiesAabbOffset);
		context.cmd.DispatchCompute(data.CameraCullingShader, m_KernelCameraCull, RenderingUtils.DivRoundUp(data.GpuData.BodiesCountWithoutGrass, 64), 1, 1);
		data.MatrixPool.Return(array);
	}

	private void SimulationStep(SimulationPassData data, RenderGraphContext context)
	{
		data.GpuData.BodyGroupsOffsetCount.TryGetValue(BodyGroups.GroupGrass, out var value);
		foreach (KeyValuePair<BodyGroups, int2> item in data.GpuData.BodyGroupsOffsetCount)
		{
			int num = data.KernelSimulate32;
			int x = item.Value.x;
			int num2 = item.Value.y;
			if (num2 > 0)
			{
				switch (item.Key)
				{
				case BodyGroups.GroupGrass:
				{
					num = data.KernelSimulateGrass;
					data.SimulationShader.GetKernelThreadGroupSizes(num, out var x2, out var _, out var _);
					num2 = RenderingUtils.DivRoundUp(data.GpuData.GrassParticlesCount, (int)x2);
					break;
				}
				case BodyGroups.Group32:
					num = data.KernelSimulate32;
					break;
				case BodyGroups.Group64:
					num = data.KernelSimulate64;
					break;
				case BodyGroups.Group128:
					num = data.KernelSimulate128;
					break;
				case BodyGroups.Group192:
					num = data.KernelSimulate192;
					break;
				case BodyGroups.Group256:
					num = data.KernelSimulate256;
					break;
				case BodyGroups.Group512:
					num = data.KernelSimulate512;
					break;
				}
				if (num != data.KernelSimulateGrass)
				{
					data.GpuData.DisconnectedConstraintsGroupsSoA.SetComputeData(context.cmd, data.SimulationShader, num);
				}
				data.GpuData.BodyDescriptorsSoA.SetComputeData(context.cmd, data.SimulationShader, num);
				data.GpuData.BodyGroupsIndicesSoA.SetComputeData(context.cmd, data.SimulationShader, num);
				context.cmd.SetComputeIntParam(data.SimulationShader, PositionBasedDynamicsConstantBuffer._GrassBodyCount, value.y);
				context.cmd.SetComputeIntParam(data.SimulationShader, PositionBasedDynamicsConstantBuffer._BodyGroupOffset, x);
				context.cmd.SetComputeIntParam(data.SimulationShader, PositionBasedDynamicsConstantBuffer._BodiesAabbOffset, data.Broadphase.BodiesAabbOffset);
				context.cmd.DispatchCompute(data.SimulationShader, num, num2, 1, 1);
			}
		}
	}

	private void Skinning(SimulationPassData data, RenderGraphContext context)
	{
		if (data.GpuData.SkinnedBodyDescriptorsMap.Count <= 0)
		{
			return;
		}
		data.GpuData.SkinnedBodyDescriptorsIndicesSoA.SetComputeData(context.cmd, data.SkinningShader, data.KernelSkinnedUpdateBindposesSingleDispatch);
		int num = 0;
		int num2 = 0;
		Matrix4x4[] array = data.MatrixPool.Rent(256);
		Matrix4x4[] array2 = data.MatrixPool.Rent(256);
		foreach (KeyValuePair<SkinnedBody, int> item in data.GpuData.SkinnedBodyDescriptorsMap)
		{
			array[num] = item.Key.LocalToWorld;
			array2[num] = item.Key.LocalToWorld.inverse;
			num++;
			if (num >= 256 || num + num2 >= data.GpuData.SkinnedBodyDescriptorsMap.Count)
			{
				context.cmd.SetComputeIntParam(data.SkinningShader, PositionBasedDynamicsConstantBuffer._Offset, num2);
				context.cmd.SetComputeMatrixArrayParam(data.SkinningShader, PositionBasedDynamicsConstantBuffer._WorldMatrices, array);
				context.cmd.SetComputeMatrixArrayParam(data.SkinningShader, PositionBasedDynamicsConstantBuffer._InvWorldMatrices, array2);
				context.cmd.DispatchCompute(data.SkinningShader, data.KernelSkinnedUpdateBindposesSingleDispatch, num, 1, 1);
				num2 += 256;
				num = 0;
			}
		}
		context.cmd.SetGlobalBuffer(PositionBasedDynamicsConstantBuffer._PbdBindposes, data.GpuData.SkinnedBodySoA.SimulatedBindposesBuffer);
		context.cmd.SetGlobalBuffer(PositionBasedDynamicsConstantBuffer._PbdSkinnedBodyBoneIndicesMap, data.GpuData.SkinnedBodyBoneIndicesMapSoA.Buffer);
		context.cmd.SetGlobalFloat(PositionBasedDynamicsConstantBuffer._PbdEnabledGlobal, 1f);
		data.MatrixPool.Return(array);
		data.MatrixPool.Return(array2);
	}

	private void Mesh(SimulationPassData data, RenderGraphContext context)
	{
		if (data.GpuData.MeshBodyDescriptorsMap.Count <= 0)
		{
			return;
		}
		data.GpuData.MeshBodyDescriptorsIndicesSoA.SetComputeData(context.cmd, data.MeshShader, data.KernelMeshUpdateNormalsAndTangentsSingleDispatch);
		int num = 0;
		int num2 = 0;
		Matrix4x4[] array = data.MatrixPool.Rent(256);
		foreach (KeyValuePair<MeshBody, int> item in data.GpuData.MeshBodyDescriptorsMap)
		{
			array[num] = item.Key.LocalToWorld.inverse;
			num++;
			if (num >= 256 || num + num2 >= data.GpuData.MeshBodyDescriptorsMap.Count)
			{
				context.cmd.SetComputeIntParam(data.MeshShader, PositionBasedDynamicsConstantBuffer._Offset, num2);
				context.cmd.SetComputeMatrixArrayParam(data.MeshShader, PositionBasedDynamicsConstantBuffer._InvWorldMatrices, array);
				context.cmd.DispatchCompute(data.MeshShader, data.KernelMeshUpdateNormalsAndTangentsSingleDispatch, num, 1, 1);
				num2 += num;
				num = 0;
			}
		}
		context.cmd.SetGlobalBuffer(PositionBasedDynamicsConstantBuffer._PBDNormals, data.GpuData.MeshBodyVerticesSoA.NormalsBuffer);
		context.cmd.SetGlobalBuffer(PositionBasedDynamicsConstantBuffer._PBDTangents, data.GpuData.MeshBodyVerticesSoA.TangentsBuffer);
		context.cmd.SetGlobalFloat(PositionBasedDynamicsConstantBuffer._PbdEnabledGlobal, 1f);
		data.MatrixPool.Return(array);
	}
}
