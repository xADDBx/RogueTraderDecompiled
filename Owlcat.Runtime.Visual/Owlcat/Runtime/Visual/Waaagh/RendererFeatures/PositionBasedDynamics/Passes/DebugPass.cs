using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics.Passes;

public class DebugPass : ScriptableRenderPass<DebugPassData>
{
	private static int _ParticleIndicesBuffer = Shader.PropertyToID("_ParticleIndicesBuffer");

	private static int _ConstraintIndicesBuffer = Shader.PropertyToID("_ConstraintIndicesBuffer");

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

	public override string Name => "DebugPass";

	public DebugPass(RenderPassEvent evt, PositionBasedDynamicsFeature feature, Material debugMaterial)
		: base(evt)
	{
		m_Feature = feature;
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

	protected override void Setup(RenderGraphBuilder builder, DebugPassData data, ref RenderingData renderingData)
	{
		m_GpuData = PBD.GetGPUData();
		if (!PBD.IsSceneInitialization || m_Feature.DebugSoA == null || !m_GpuData.IsValid)
		{
			UpdateDebugBuffers();
		}
		data.Feature = m_Feature;
		data.DebugMaterial = m_DebugMaterial;
		data.GpuData = m_GpuData;
		data.Camera = renderingData.CameraData.Camera;
		data.CameraColorRT = builder.WriteTexture(in data.Resources.CameraColorBuffer);
		data.DrawParticlesArgs = m_DrawParticlesArgs;
		data.DrawConstraintsArgs = m_DrawConstraintsArgs;
		data.DrawNormalsArgs = m_DrawNormalsArgs;
		data.DrawCollidersGridArgs = m_DrawCollidersGridArgs;
		data.DrawCollidersAabbArgs = m_DrawCollidersAabbArgs;
		data.DrawForceVolumesAabbArgs = m_DrawForceVolumesAabbArgs;
		data.DrawForceVolumesGridArgs = m_DrawForceVolumesGridArgs;
		data.DrawBodiesAabbArgs = m_DrawBodiesAabbArgs;
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

	protected override void Render(DebugPassData data, RenderGraphContext context)
	{
		if (m_Feature.DebugSoA.DrawParticlesArgsBuffer == null || !m_Feature.DebugSoA.DrawParticlesArgsBuffer.IsValid() || PBD.IsSceneInitialization || !m_GpuData.IsValid)
		{
			return;
		}
		context.renderContext.SetupCameraProperties(data.Camera);
		context.cmd.SetRenderTarget(data.CameraColorRT);
		context.cmd.SetGlobalBuffer(_ParticleIndicesBuffer, data.Feature.DebugSoA.DebugParticleIndicesBuffer);
		context.cmd.SetGlobalBuffer(_ConstraintIndicesBuffer, data.Feature.DebugSoA.DebugDistanceConstraintsIndicesBuffer);
		data.GpuData.ParticlesSoA.SetGlobalData(context.cmd);
		data.GpuData.ConstraintsSoA.SetGlobalData(context.cmd);
		context.cmd.SetGlobalMatrix(PositionBasedDynamicsConstantBuffer._World2Local, Matrix4x4.identity);
		context.cmd.SetGlobalMatrix(PositionBasedDynamicsConstantBuffer._Local2World, Matrix4x4.identity);
		context.cmd.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, data.Feature.Config.DebugSettings.ConstraintColor);
		if (data.GpuData.ConstraintsCount > 0)
		{
			context.cmd.DrawMeshInstancedIndirect(RenderingUtils.CubeMesh, 0, data.DebugMaterial, 1, data.Feature.DebugSoA.DrawDistanceConstraintsArgsBuffer);
		}
		if (data.Feature.Config.DebugSettings.ShowNormals)
		{
			foreach (KeyValuePair<MeshBody, int> item in data.GpuData.MeshBodyDescriptorsMap)
			{
				data.DrawNormalsArgs[1] = (uint)item.Key.Vertices.Count;
				data.Feature.DebugSoA.DrawNormalsArgsBuffer.SetData(data.DrawNormalsArgs);
				context.cmd.SetGlobalMatrix(PositionBasedDynamicsConstantBuffer._Local2World, item.Key.LocalToWorld);
				context.cmd.SetGlobalMatrix(PositionBasedDynamicsConstantBuffer._World2Local, item.Key.LocalToWorld.inverse);
				context.cmd.SetGlobalInt(PositionBasedDynamicsConstantBuffer._ParticlesOffset, item.Key.ParticlesOffset);
				context.cmd.SetGlobalInt(PositionBasedDynamicsConstantBuffer._VerticesOffset, item.Key.VertexOffset);
				context.cmd.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, data.Feature.Config.DebugSettings.NormalsColor);
				context.cmd.SetGlobalBuffer(PositionBasedDynamicsConstantBuffer._PBDNormals, data.GpuData.MeshBodyVerticesSoA.NormalsBuffer);
				context.cmd.DrawMeshInstancedIndirect(RenderingUtils.CubeMesh, 0, m_DebugMaterial, 2, data.Feature.DebugSoA.DrawNormalsArgsBuffer);
			}
		}
		context.cmd.SetGlobalMatrix(PositionBasedDynamicsConstantBuffer._World2Local, Matrix4x4.identity);
		context.cmd.SetGlobalMatrix(PositionBasedDynamicsConstantBuffer._Local2World, Matrix4x4.identity);
		context.cmd.SetGlobalFloat(PositionBasedDynamicsConstantBuffer._Size, data.Feature.Config.DebugSettings.ParticleSize);
		context.cmd.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, data.Feature.Config.DebugSettings.ParticleColor);
		if (data.GpuData.ParticlesCount > 0)
		{
			context.cmd.DrawMeshInstancedIndirect(RenderingUtils.CubeMesh, 0, data.DebugMaterial, 0, data.Feature.DebugSoA.DrawParticlesArgsBuffer);
		}
		if (PBD.DebugSettings.ShowBoundingBoxes)
		{
			data.Feature.Broadphase.BoundingBoxSoA.SetGlobalData(context.cmd);
			if (data.Feature.Broadphase.BodiesCount > 0)
			{
				context.cmd.SetGlobalInt("_IsDrawingBodyAabb", 1);
				context.cmd.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, PBD.DebugSettings.BodyColor);
				context.cmd.SetGlobalInt(PositionBasedDynamicsConstantBuffer._DebugAabbOffset, data.Feature.Broadphase.BodiesAabbOffset);
				context.cmd.DrawMeshInstancedIndirect(RenderingUtils.CubeMeshWithUvAndNormals, 0, data.DebugMaterial, 3, data.Feature.DebugSoA.DrawBodiesAabbArgsBuffer);
			}
			context.cmd.SetGlobalInt("_IsDrawingBodyAabb", 0);
			if (data.Feature.Broadphase.CollidersCount > 0 && data.Feature.DebugSoA.DrawCollidersAabbArgsBuffer != null)
			{
				context.cmd.SetGlobalInt(PositionBasedDynamicsConstantBuffer._DebugAabbOffset, data.Feature.Broadphase.CollidersAabbOffset);
				context.cmd.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, PBD.DebugSettings.ColliderColor);
				context.cmd.DrawMeshInstancedIndirect(RenderingUtils.CubeMeshWithUvAndNormals, 0, data.DebugMaterial, 3, data.Feature.DebugSoA.DrawCollidersAabbArgsBuffer);
			}
			if (data.Feature.Broadphase.ForceVolumesCount > 0)
			{
				context.cmd.SetGlobalInt(PositionBasedDynamicsConstantBuffer._DebugAabbOffset, data.Feature.Broadphase.ForceVolumesAabbOffset);
				context.cmd.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, PBD.DebugSettings.ForceVolumeColor);
				context.cmd.DrawMeshInstancedIndirect(RenderingUtils.CubeMeshWithUvAndNormals, 0, data.DebugMaterial, 3, data.Feature.DebugSoA.DrawForceVolumeAabbArgsBuffer);
			}
		}
		if (!PBD.DebugSettings.ShowBroadphaseStructure)
		{
			return;
		}
		switch (data.Feature.Broadphase.Type)
		{
		case BroadphaseType.SimpleGrid:
			context.cmd.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, PBD.DebugSettings.BroadphaseStructureColor);
			context.cmd.SetGlobalColor(PositionBasedDynamicsConstantBuffer._ColorPair, PBD.DebugSettings.BodyColliderPairColor);
			context.cmd.SetGlobalInt(PositionBasedDynamicsConstantBuffer._BroadphaseGridResolution, PBD.BroadphaseSettings.SimpleGridSettings.GridResolution);
			context.cmd.DrawMeshInstancedIndirect(RenderingUtils.CubeMeshWithUvAndNormals, 0, data.DebugMaterial, 4, data.Feature.DebugSoA.DrawCollidersGridArgsBuffer);
			if (data.GpuData.ForceVolumes != null && data.GpuData.ForceVolumes.Count > 0 && data.Feature.DebugSoA.DrawForceVolumeGridArgsBuffer != null)
			{
				context.cmd.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, PBD.DebugSettings.BroadphaseStructureColor);
				context.cmd.SetGlobalColor(PositionBasedDynamicsConstantBuffer._ColorPair, PBD.DebugSettings.BodyForceVolumePairColor);
				context.cmd.SetGlobalInt(PositionBasedDynamicsConstantBuffer._BroadphaseGridResolution, PBD.BroadphaseSettings.SimpleGridSettings.GridResolution);
				context.cmd.DrawMeshInstancedIndirect(RenderingUtils.CubeMeshWithUvAndNormals, 0, data.DebugMaterial, 5, data.Feature.DebugSoA.DrawForceVolumeGridArgsBuffer);
			}
			break;
		case BroadphaseType.OptimizedSpatialHashing:
			if (data.Feature.Broadphase.BodyColliderPairsSoA.Length > 0)
			{
				context.cmd.SetGlobalInt(PositionBasedDynamicsConstantBuffer._BodiesAabbOffset, data.Feature.Broadphase.BodiesAabbOffset);
				context.cmd.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, PBD.DebugSettings.BodyColliderPairColor);
				context.cmd.DrawMeshInstancedIndirect(RenderingUtils.CubeMeshWithUvAndNormals, 0, data.DebugMaterial, 6, data.Feature.DebugSoA.DrawCollidersGridArgsBuffer);
			}
			if (data.Feature.Broadphase.BodyForceVolumePairsSoA.Length > 0)
			{
				context.cmd.SetGlobalInt(PositionBasedDynamicsConstantBuffer._BodiesAabbOffset, data.Feature.Broadphase.BodiesAabbOffset);
				context.cmd.SetGlobalInt(PositionBasedDynamicsConstantBuffer._ForceVolumesAabbOffset, data.Feature.Broadphase.ForceVolumesAabbOffset);
				context.cmd.SetGlobalColor(PositionBasedDynamicsConstantBuffer._Color, PBD.DebugSettings.BodyForceVolumePairColor);
				context.cmd.DrawMeshInstancedIndirect(RenderingUtils.CubeMeshWithUvAndNormals, 0, data.DebugMaterial, 7, data.Feature.DebugSoA.DrawForceVolumeGridArgsBuffer);
			}
			break;
		case BroadphaseType.MultilevelGrid:
			break;
		}
	}
}
