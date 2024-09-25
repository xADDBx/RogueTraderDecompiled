using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Passes;

public class DebugPass : ScriptableRenderPass
{
	private const string kProfilerTag = "PBD Debug Pass";

	private static int _ParticleIndicesBuffer = Shader.PropertyToID("_ParticleIndicesBuffer");

	private static int _ConstraintIndicesBuffer = Shader.PropertyToID("_ConstraintIndicesBuffer");

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("PBD Debug Pass");

	private RenderTargetHandle m_ColorAttachment;

	private PositionBasedDynamicsFeature m_Feature;

	private Material m_DebugMaterial;

	protected uint[] m_DrawParticlesArgs = new uint[5];

	protected uint[] m_DrawConstraintsArgs = new uint[5];

	protected uint[] m_DrawNormalsArgs = new uint[5];

	protected uint[] m_DrawCollidersGridArgs = new uint[5];

	protected uint[] m_DrawCollidersAabbArgs = new uint[5];

	protected uint[] m_DrawForceVolumesAabbArgs = new uint[5];

	protected uint[] m_DrawForceVolumesGridArgs = new uint[5];

	protected uint[] m_DrawBodiesAabbArgs = new uint[5];

	private GPUData m_GpuData;

	private int m_LastGpuDataVersion = -1;

	public DebugPass(RenderPassEvent evt, Material debugMaterial)
	{
		base.RenderPassEvent = evt;
		m_DebugMaterial = debugMaterial;
		m_DrawParticlesArgs[0] = RenderingUtils.CubeMesh.GetIndexCount(0);
		m_DrawConstraintsArgs[0] = RenderingUtils.CubeMesh.GetIndexCount(0);
		m_DrawNormalsArgs[0] = RenderingUtils.CubeMesh.GetIndexCount(0);
		m_DrawCollidersGridArgs[0] = RenderingUtils.CubeMeshWithUvAndNormals.GetIndexCount(0);
		m_DrawCollidersAabbArgs[0] = RenderingUtils.CubeMeshWithUvAndNormals.GetIndexCount(0);
		m_DrawForceVolumesAabbArgs[0] = RenderingUtils.CubeMeshWithUvAndNormals.GetIndexCount(0);
		m_DrawForceVolumesGridArgs[0] = RenderingUtils.CubeMeshWithUvAndNormals.GetIndexCount(0);
		m_DrawBodiesAabbArgs[0] = RenderingUtils.CubeMeshWithUvAndNormals.GetIndexCount(0);
	}

	public void Setup(PositionBasedDynamicsFeature feature, RenderTargetHandle colorAttachment)
	{
		m_Feature = feature;
		m_ColorAttachment = colorAttachment;
		m_GpuData = PBD.GetGPUData();
		if (!PBD.IsSceneInitialization || m_Feature.DebugSoA == null || !m_GpuData.IsValid)
		{
			UpdateDebugBuffers();
			m_LastGpuDataVersion = m_GpuData.Version;
		}
	}

	private void UpdateDebugBuffers()
	{
		List<int> list = ListPool<int>.Get();
		List<int> list2 = ListPool<int>.Get();
		foreach (KeyValuePair<Body, int> item in m_GpuData.BodyDescriptorsMap)
		{
			int value = item.Value;
			BodyDescriptor bodyDescriptor = m_GpuData.CPUBodyDescriptors[value];
			for (int i = 0; i < bodyDescriptor.ParticlesOffsetCount.y; i++)
			{
				list.Add(bodyDescriptor.ParticlesOffsetCount.x + i);
			}
			for (int j = 0; j < bodyDescriptor.ConstraintsOffsetCount.y; j++)
			{
				list2.Add(bodyDescriptor.ConstraintsOffsetCount.x + j);
			}
		}
		if (m_Feature.DebugSoA.DebugParticleIndicesBuffer == null || m_Feature.DebugSoA.DebugParticleIndicesBuffer.count < list.Count)
		{
			if (m_Feature.DebugSoA.DebugParticleIndicesBuffer != null)
			{
				m_Feature.DebugSoA.DebugParticleIndicesBuffer.Release();
			}
			if (list.Count > 0)
			{
				m_Feature.DebugSoA.DebugParticleIndicesBuffer = new ComputeBuffer(list.Count, 4, ComputeBufferType.Structured);
			}
		}
		if (list.Count > 0)
		{
			m_Feature.DebugSoA.DebugParticleIndicesBuffer.SetData(list);
		}
		if (m_Feature.DebugSoA.DebugDistanceConstraintsIndicesBuffer == null || m_Feature.DebugSoA.DebugDistanceConstraintsIndicesBuffer.count < list2.Count)
		{
			if (m_Feature.DebugSoA.DebugDistanceConstraintsIndicesBuffer != null)
			{
				m_Feature.DebugSoA.DebugDistanceConstraintsIndicesBuffer.Release();
			}
			if (list2.Count > 0)
			{
				m_Feature.DebugSoA.DebugDistanceConstraintsIndicesBuffer = new ComputeBuffer(list2.Count, 4, ComputeBufferType.Structured);
			}
		}
		if (list2.Count > 0)
		{
			m_Feature.DebugSoA.DebugDistanceConstraintsIndicesBuffer.SetData(list2);
		}
		ListPool<int>.Release(list);
		ListPool<int>.Release(list2);
		if (m_DrawParticlesArgs[1] != m_GpuData.ParticlesCount)
		{
			m_DrawParticlesArgs[1] = (uint)m_GpuData.ParticlesCount;
			if (m_Feature.DebugSoA.DrawParticlesArgsBuffer != null)
			{
				m_Feature.DebugSoA.DrawParticlesArgsBuffer.Release();
			}
			m_Feature.DebugSoA.DrawParticlesArgsBuffer = new ComputeBuffer(1, 4 * m_DrawParticlesArgs.Length, ComputeBufferType.DrawIndirect);
			m_Feature.DebugSoA.DrawParticlesArgsBuffer.name = "PBD.DrawParticlesDebugArgsBuffer";
			m_Feature.DebugSoA.DrawParticlesArgsBuffer.SetData(m_DrawParticlesArgs);
		}
		if (m_DrawConstraintsArgs[1] != m_GpuData.ConstraintsCount)
		{
			m_DrawConstraintsArgs[1] = (uint)m_GpuData.ConstraintsCount;
			if (m_Feature.DebugSoA.DrawDistanceConstraintsArgsBuffer != null)
			{
				m_Feature.DebugSoA.DrawDistanceConstraintsArgsBuffer.Release();
			}
			m_Feature.DebugSoA.DrawDistanceConstraintsArgsBuffer = new ComputeBuffer(1, 4 * m_DrawConstraintsArgs.Length, ComputeBufferType.DrawIndirect);
			m_Feature.DebugSoA.DrawDistanceConstraintsArgsBuffer.name = "PBD.DrawDistanceConstraintsArgsBuffer";
			m_Feature.DebugSoA.DrawDistanceConstraintsArgsBuffer.SetData(m_DrawConstraintsArgs);
		}
		if (m_Feature.DebugSoA.DrawNormalsArgsBuffer == null)
		{
			m_Feature.DebugSoA.DrawNormalsArgsBuffer = new ComputeBuffer(1, 4 * m_DrawNormalsArgs.Length, ComputeBufferType.DrawIndirect);
			m_Feature.DebugSoA.DrawNormalsArgsBuffer.name = "PBD.DrawNormalsArgsBuffer";
			m_Feature.DebugSoA.DrawNormalsArgsBuffer.SetData(m_DrawNormalsArgs);
		}
		switch (m_Feature.Broadphase.Type)
		{
		case BroadphaseType.SimpleGrid:
		{
			int num = PBD.BroadphaseSettings.SimpleGridSettings.GridResolution * PBD.BroadphaseSettings.SimpleGridSettings.GridResolution;
			if (m_DrawCollidersGridArgs[1] != num)
			{
				m_DrawCollidersGridArgs[1] = (uint)num;
				if (m_Feature.DebugSoA.DrawCollidersGridArgsBuffer != null)
				{
					m_Feature.DebugSoA.DrawCollidersGridArgsBuffer.Release();
				}
				m_Feature.DebugSoA.DrawCollidersGridArgsBuffer = new ComputeBuffer(1, 4 * m_DrawCollidersGridArgs.Length, ComputeBufferType.DrawIndirect);
				m_Feature.DebugSoA.DrawCollidersGridArgsBuffer.name = "PBD.DrawCollidersGridArgsBuffer";
				m_Feature.DebugSoA.DrawCollidersGridArgsBuffer.SetData(m_DrawCollidersGridArgs);
			}
			int num2 = PBD.BroadphaseSettings.SimpleGridSettings.GridResolution * PBD.BroadphaseSettings.SimpleGridSettings.GridResolution;
			if (m_DrawForceVolumesGridArgs[1] != num2)
			{
				m_DrawForceVolumesGridArgs[1] = (uint)num2;
				if (m_Feature.DebugSoA.DrawForceVolumeGridArgsBuffer != null)
				{
					m_Feature.DebugSoA.DrawForceVolumeGridArgsBuffer.Release();
				}
				m_Feature.DebugSoA.DrawForceVolumeGridArgsBuffer = new ComputeBuffer(1, 4 * m_DrawForceVolumesGridArgs.Length, ComputeBufferType.DrawIndirect);
				m_Feature.DebugSoA.DrawForceVolumeGridArgsBuffer.name = "PBD.DrawForceVolumeGridArgsBuffer";
				m_Feature.DebugSoA.DrawForceVolumeGridArgsBuffer.SetData(m_DrawForceVolumesGridArgs);
			}
			break;
		}
		case BroadphaseType.OptimizedSpatialHashing:
		{
			int length = m_Feature.Broadphase.BodyColliderPairsSoA.Length;
			if (m_DrawCollidersGridArgs[1] != length)
			{
				m_DrawCollidersGridArgs[1] = (uint)length;
				if (m_Feature.DebugSoA.DrawCollidersGridArgsBuffer != null)
				{
					m_Feature.DebugSoA.DrawCollidersGridArgsBuffer.Release();
				}
				if (length > 0)
				{
					m_Feature.DebugSoA.DrawCollidersGridArgsBuffer = new ComputeBuffer(1, 4 * m_DrawCollidersGridArgs.Length, ComputeBufferType.DrawIndirect);
					m_Feature.DebugSoA.DrawCollidersGridArgsBuffer.name = "PBD.DrawCollidersGridArgsBuffer";
					m_Feature.DebugSoA.DrawCollidersGridArgsBuffer.SetData(m_DrawCollidersGridArgs);
				}
			}
			int length2 = m_Feature.Broadphase.BodyForceVolumePairsSoA.Length;
			if (m_DrawForceVolumesGridArgs[1] != length2)
			{
				m_DrawForceVolumesGridArgs[1] = (uint)length2;
				if (m_Feature.DebugSoA.DrawForceVolumeGridArgsBuffer != null)
				{
					m_Feature.DebugSoA.DrawForceVolumeGridArgsBuffer.Release();
				}
				m_Feature.DebugSoA.DrawForceVolumeGridArgsBuffer = new ComputeBuffer(1, 4 * m_DrawForceVolumesGridArgs.Length, ComputeBufferType.DrawIndirect);
				m_Feature.DebugSoA.DrawForceVolumeGridArgsBuffer.name = "PBD.DrawForceVolumeGridArgsBuffer";
				m_Feature.DebugSoA.DrawForceVolumeGridArgsBuffer.SetData(m_DrawForceVolumesGridArgs);
			}
			break;
		}
		}
		if (m_Feature.Broadphase.CollidersCount > 0 && m_DrawCollidersAabbArgs[1] != m_Feature.Broadphase.CollidersCount)
		{
			m_DrawCollidersAabbArgs[1] = (uint)m_Feature.Broadphase.CollidersCount;
			if (m_Feature.DebugSoA.DrawCollidersAabbArgsBuffer != null)
			{
				m_Feature.DebugSoA.DrawCollidersAabbArgsBuffer.Release();
			}
			m_Feature.DebugSoA.DrawCollidersAabbArgsBuffer = new ComputeBuffer(1, 4 * m_DrawCollidersAabbArgs.Length, ComputeBufferType.DrawIndirect);
			m_Feature.DebugSoA.DrawCollidersAabbArgsBuffer.name = "PBD.DrawCollidersAabbArgsBuffer";
			m_Feature.DebugSoA.DrawCollidersAabbArgsBuffer.SetData(m_DrawCollidersAabbArgs);
		}
		if (m_GpuData.ForceVolumes != null && m_GpuData.ForceVolumes.Count > 0 && m_DrawForceVolumesAabbArgs[1] != m_GpuData.ForceVolumes.Count)
		{
			m_DrawForceVolumesAabbArgs[1] = (uint)m_GpuData.ForceVolumes.Count;
			if (m_Feature.DebugSoA.DrawForceVolumeAabbArgsBuffer != null)
			{
				m_Feature.DebugSoA.DrawForceVolumeAabbArgsBuffer.Release();
			}
			m_Feature.DebugSoA.DrawForceVolumeAabbArgsBuffer = new ComputeBuffer(1, 4 * m_DrawForceVolumesAabbArgs.Length, ComputeBufferType.DrawIndirect);
			m_Feature.DebugSoA.DrawForceVolumeAabbArgsBuffer.name = "PBD.DrawForceVolumesAabbArgsBuffer";
			m_Feature.DebugSoA.DrawForceVolumeAabbArgsBuffer.SetData(m_DrawForceVolumesAabbArgs);
		}
		int bodiesCountWithoutGrass = m_GpuData.BodiesCountWithoutGrass;
		if (m_DrawBodiesAabbArgs[1] != bodiesCountWithoutGrass && bodiesCountWithoutGrass > 0)
		{
			m_DrawBodiesAabbArgs[1] = (uint)bodiesCountWithoutGrass;
			if (m_Feature.DebugSoA.DrawBodiesAabbArgsBuffer != null)
			{
				m_Feature.DebugSoA.DrawBodiesAabbArgsBuffer.Release();
			}
			m_Feature.DebugSoA.DrawBodiesAabbArgsBuffer = new ComputeBuffer(1, 4 * m_DrawBodiesAabbArgs.Length, ComputeBufferType.DrawIndirect);
			m_Feature.DebugSoA.DrawBodiesAabbArgsBuffer.name = "PBD.DrawBodiesAabbArgsBuffer";
			m_Feature.DebugSoA.DrawBodiesAabbArgsBuffer.SetData(m_DrawBodiesAabbArgs);
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (m_Feature.DebugSoA.DrawParticlesArgsBuffer == null || !m_Feature.DebugSoA.DrawParticlesArgsBuffer.IsValid() || PBD.IsSceneInitialization || !m_GpuData.IsValid)
		{
			return;
		}
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			context.SetupCameraProperties(renderingData.CameraData.Camera);
			commandBuffer.SetRenderTarget(m_ColorAttachment.Identifier());
			commandBuffer.SetGlobalBuffer(_ParticleIndicesBuffer, m_Feature.DebugSoA.DebugParticleIndicesBuffer);
			commandBuffer.SetGlobalBuffer(_ConstraintIndicesBuffer, m_Feature.DebugSoA.DebugDistanceConstraintsIndicesBuffer);
			m_GpuData.ParticlesSoA.SetGlobalData(commandBuffer);
			m_GpuData.ConstraintsSoA.SetGlobalData(commandBuffer);
			commandBuffer.SetGlobalMatrix(PositionBasedDynamicsConstantBuffer._World2Local, Matrix4x4.identity);
			commandBuffer.SetGlobalMatrix(PositionBasedDynamicsConstantBuffer._Local2World, Matrix4x4.identity);
			commandBuffer.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, m_Feature.DebugSettings.ConstraintColor);
			if (m_GpuData.ConstraintsCount > 0)
			{
				commandBuffer.DrawMeshInstancedIndirect(RenderingUtils.CubeMesh, 0, m_DebugMaterial, 1, m_Feature.DebugSoA.DrawDistanceConstraintsArgsBuffer);
			}
			if (m_Feature.DebugSettings.ShowNormals)
			{
				foreach (KeyValuePair<MeshBody, int> item in m_GpuData.MeshBodyDescriptorsMap)
				{
					m_DrawNormalsArgs[1] = (uint)item.Key.Vertices.Count;
					m_Feature.DebugSoA.DrawNormalsArgsBuffer.SetData(m_DrawNormalsArgs);
					commandBuffer.SetGlobalMatrix(PositionBasedDynamicsConstantBuffer._Local2World, item.Key.LocalToWorld);
					commandBuffer.SetGlobalMatrix(PositionBasedDynamicsConstantBuffer._World2Local, item.Key.LocalToWorld.inverse);
					commandBuffer.SetGlobalInt(PositionBasedDynamicsConstantBuffer._ParticlesOffset, item.Key.ParticlesOffset);
					commandBuffer.SetGlobalInt(PositionBasedDynamicsConstantBuffer._VerticesOffset, item.Key.VertexOffset);
					commandBuffer.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, m_Feature.DebugSettings.NormalsColor);
					commandBuffer.SetGlobalBuffer(PositionBasedDynamicsConstantBuffer._PBDNormals, m_GpuData.MeshBodyVerticesSoA.NormalsBuffer);
					commandBuffer.DrawMeshInstancedIndirect(RenderingUtils.CubeMesh, 0, m_DebugMaterial, 2, m_Feature.DebugSoA.DrawNormalsArgsBuffer);
				}
			}
			commandBuffer.SetGlobalMatrix(PositionBasedDynamicsConstantBuffer._World2Local, Matrix4x4.identity);
			commandBuffer.SetGlobalMatrix(PositionBasedDynamicsConstantBuffer._Local2World, Matrix4x4.identity);
			commandBuffer.SetGlobalFloat(PositionBasedDynamicsConstantBuffer._Size, m_Feature.DebugSettings.ParticleSize);
			commandBuffer.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, m_Feature.DebugSettings.ParticleColor);
			if (m_GpuData.ParticlesCount > 0)
			{
				commandBuffer.DrawMeshInstancedIndirect(RenderingUtils.CubeMesh, 0, m_DebugMaterial, 0, m_Feature.DebugSoA.DrawParticlesArgsBuffer);
			}
			if (PBD.DebugSettings.ShowBoundingBoxes)
			{
				m_Feature.Broadphase.BoundingBoxSoA.SetGlobalData(commandBuffer);
				if (m_Feature.Broadphase.BodiesCount > 0)
				{
					commandBuffer.SetGlobalInt("_IsDrawingBodyAabb", 1);
					commandBuffer.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, PBD.DebugSettings.BodyColor);
					commandBuffer.SetGlobalInt(PositionBasedDynamicsConstantBuffer._DebugAabbOffset, m_Feature.Broadphase.BodiesAabbOffset);
					commandBuffer.DrawMeshInstancedIndirect(RenderingUtils.CubeMeshWithUvAndNormals, 0, m_DebugMaterial, 3, m_Feature.DebugSoA.DrawBodiesAabbArgsBuffer);
				}
				commandBuffer.SetGlobalInt("_IsDrawingBodyAabb", 0);
				if (m_Feature.Broadphase.CollidersCount > 0 && m_Feature.DebugSoA.DrawCollidersAabbArgsBuffer != null)
				{
					commandBuffer.SetGlobalInt(PositionBasedDynamicsConstantBuffer._DebugAabbOffset, m_Feature.Broadphase.CollidersAabbOffset);
					commandBuffer.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, PBD.DebugSettings.ColliderColor);
					commandBuffer.DrawMeshInstancedIndirect(RenderingUtils.CubeMeshWithUvAndNormals, 0, m_DebugMaterial, 3, m_Feature.DebugSoA.DrawCollidersAabbArgsBuffer);
				}
				if (m_Feature.Broadphase.ForceVolumesCount > 0)
				{
					commandBuffer.SetGlobalInt(PositionBasedDynamicsConstantBuffer._DebugAabbOffset, m_Feature.Broadphase.ForceVolumesAabbOffset);
					commandBuffer.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, PBD.DebugSettings.ForceVolumeColor);
					commandBuffer.DrawMeshInstancedIndirect(RenderingUtils.CubeMeshWithUvAndNormals, 0, m_DebugMaterial, 3, m_Feature.DebugSoA.DrawForceVolumeAabbArgsBuffer);
				}
			}
			if (PBD.DebugSettings.ShowBroadphaseStructure)
			{
				switch (m_Feature.Broadphase.Type)
				{
				case BroadphaseType.SimpleGrid:
					commandBuffer.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, PBD.DebugSettings.BroadphaseStructureColor);
					commandBuffer.SetGlobalColor(PositionBasedDynamicsConstantBuffer._ColorPair, PBD.DebugSettings.BodyColliderPairColor);
					commandBuffer.SetGlobalInt(PositionBasedDynamicsConstantBuffer._BroadphaseGridResolution, PBD.BroadphaseSettings.SimpleGridSettings.GridResolution);
					commandBuffer.DrawMeshInstancedIndirect(RenderingUtils.CubeMeshWithUvAndNormals, 0, m_DebugMaterial, 4, m_Feature.DebugSoA.DrawCollidersGridArgsBuffer);
					if (m_GpuData.ForceVolumes != null && m_GpuData.ForceVolumes.Count > 0 && m_Feature.DebugSoA.DrawForceVolumeGridArgsBuffer != null)
					{
						commandBuffer.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, PBD.DebugSettings.BroadphaseStructureColor);
						commandBuffer.SetGlobalColor(PositionBasedDynamicsConstantBuffer._ColorPair, PBD.DebugSettings.BodyForceVolumePairColor);
						commandBuffer.SetGlobalInt(PositionBasedDynamicsConstantBuffer._BroadphaseGridResolution, PBD.BroadphaseSettings.SimpleGridSettings.GridResolution);
						commandBuffer.DrawMeshInstancedIndirect(RenderingUtils.CubeMeshWithUvAndNormals, 0, m_DebugMaterial, 5, m_Feature.DebugSoA.DrawForceVolumeGridArgsBuffer);
					}
					break;
				case BroadphaseType.OptimizedSpatialHashing:
					if (m_Feature.Broadphase.BodyColliderPairsSoA.Length > 0)
					{
						commandBuffer.SetGlobalInt(PositionBasedDynamicsConstantBuffer._BodiesAabbOffset, m_Feature.Broadphase.BodiesAabbOffset);
						commandBuffer.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, PBD.DebugSettings.BodyColliderPairColor);
						commandBuffer.DrawMeshInstancedIndirect(RenderingUtils.CubeMeshWithUvAndNormals, 0, m_DebugMaterial, 6, m_Feature.DebugSoA.DrawCollidersGridArgsBuffer);
					}
					if (m_Feature.Broadphase.BodyForceVolumePairsSoA.Length > 0)
					{
						commandBuffer.SetGlobalInt(PositionBasedDynamicsConstantBuffer._BodiesAabbOffset, m_Feature.Broadphase.BodiesAabbOffset);
						commandBuffer.SetGlobalInt(PositionBasedDynamicsConstantBuffer._ForceVolumesAabbOffset, m_Feature.Broadphase.ForceVolumesAabbOffset);
						commandBuffer.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, PBD.DebugSettings.BodyForceVolumePairColor);
						commandBuffer.DrawMeshInstancedIndirect(RenderingUtils.CubeMeshWithUvAndNormals, 0, m_DebugMaterial, 7, m_Feature.DebugSoA.DrawForceVolumeGridArgsBuffer);
					}
					break;
				case BroadphaseType.MultilevelGrid:
					break;
				}
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
