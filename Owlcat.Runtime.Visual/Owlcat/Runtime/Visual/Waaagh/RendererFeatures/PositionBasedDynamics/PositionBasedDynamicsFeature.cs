using System;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics.Broadphase;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics.Debugging;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Position Based Dynamics")]
public class PositionBasedDynamicsFeature : ScriptableRendererFeature
{
	[Serializable]
	[ReloadGroup]
	public class ShaderResources
	{
		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/PositionBasedDynamics/Shaders/PBDSingleDispatchSimulator.compute", ReloadAttribute.Package.Root)]
		public ComputeShader PBDSingleDispatchSimulatorShader;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/PositionBasedDynamics/Shaders/PBDCollision.compute", ReloadAttribute.Package.Root)]
		public ComputeShader PBDCollision;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/PositionBasedDynamics/Shaders/PBDForceVolume.compute", ReloadAttribute.Package.Root)]
		public ComputeShader PBDForceVolume;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/PositionBasedDynamics/Shaders/PBDSkinning.compute", ReloadAttribute.Package.Root)]
		public ComputeShader PBDSkinningShader;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/PositionBasedDynamics/Shaders/PBDMesh.compute", ReloadAttribute.Package.Root)]
		public ComputeShader PBDMeshShader;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/PositionBasedDynamics/Shaders/PBDBodyAabb.compute", ReloadAttribute.Package.Root)]
		public ComputeShader PBDBodyAabbShader;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/PositionBasedDynamics/Shaders/PBDDebug.shader", ReloadAttribute.Package.Root)]
		public Shader PBDDebug;

		[SerializeField]
		[Reload("Runtime/GPUParallelSort/RadixSort.compute", ReloadAttribute.Package.Root)]
		public ComputeShader RadixSortCS;

		[SerializeField]
		[Reload("Runtime/GPUHashtable/LinearProbingHashtable.compute", ReloadAttribute.Package.Root)]
		public ComputeShader HashtableCS;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/PositionBasedDynamics/Shaders/OptimizedSpatialHashingBroadphase.compute", ReloadAttribute.Package.Root)]
		public ComputeShader SpatialHashingCS;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/PositionBasedDynamics/Shaders/PBDCameraCulling.compute", ReloadAttribute.Package.Root)]
		public ComputeShader CameraCullingCS;
	}

	[SerializeField]
	private ShaderResources m_Shaders;

	private PositionBasedDynamicsConfig m_Config;

	private GPUBroadphaseBase m_Broadphase;

	private GPUDebugSoA m_DebugSoA;

	private Material m_DebugMaterial;

	private SimulationPass m_SimulationPass;

	private DebugPass m_DebugPass;

	internal GPUBroadphaseBase Broadphase => m_Broadphase;

	internal GPUDebugSoA DebugSoA => m_DebugSoA;

	internal PositionBasedDynamicsConfig Config => m_Config;

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		CameraType cameraType = renderingData.CameraData.Camera.cameraType;
		if (cameraType == CameraType.Preview || cameraType == CameraType.Reflection)
		{
			DisableFeature();
			return;
		}
		if (!Application.isPlaying)
		{
			DisableFeature();
			return;
		}
		if (m_Config == null)
		{
			m_Config = PositionBasedDynamicsConfig.Instance;
		}
		if (!m_Config.GPU || !m_Config.Enabled || PBD.IsEmpty)
		{
			DisableFeature();
			return;
		}
		if (PBD.GetGPUData() == null)
		{
			DisableFeature();
			return;
		}
		if (m_Broadphase == null || (m_Broadphase.Type != PBD.BroadphaseSettings.Type && PBD.BroadphaseSettings.Type != BroadphaseType.MultilevelGrid))
		{
			ResetBroadphase();
		}
		m_SimulationPass.Init(m_Broadphase, m_Config.SimulationIterations, m_Config.ConstraintIterations, m_Config.Decay);
		renderer.EnqueuePass(m_SimulationPass);
		if (m_Config.DebugSettings.Enabled && !PBD.IsSceneInitialization)
		{
			if (m_DebugSoA == null)
			{
				m_DebugSoA = new GPUDebugSoA();
			}
			renderer.EnqueuePass(m_DebugPass);
		}
	}

	private void ResetBroadphase()
	{
		if (m_Broadphase != null)
		{
			m_Broadphase.Dispose();
		}
		switch (PBD.BroadphaseSettings.Type)
		{
		case BroadphaseType.SimpleGrid:
			m_Broadphase = new GPUSimpleGridBroadphase(PBD.BroadphaseSettings.SimpleGridSettings, m_Shaders.PBDCollision, m_Shaders.PBDForceVolume);
			break;
		case BroadphaseType.MultilevelGrid:
			Debug.LogWarning("Multilevel grid is not implemented on GPU. Switch to OptimizedSpatialHashing broadphase");
			m_Broadphase = new GPUOptimizedSpatialHashingBroadphase(PBD.BroadphaseSettings.OptimizedSpatialHashingSettings, m_Shaders.SpatialHashingCS);
			break;
		case BroadphaseType.OptimizedSpatialHashing:
			m_Broadphase = new GPUOptimizedSpatialHashingBroadphase(PBD.BroadphaseSettings.OptimizedSpatialHashingSettings, m_Shaders.SpatialHashingCS);
			break;
		}
	}

	public override void Create()
	{
		m_DebugMaterial = CoreUtils.CreateEngineMaterial(m_Shaders.PBDDebug);
		m_SimulationPass = new SimulationPass((RenderPassEvent)11, m_Shaders.PBDSingleDispatchSimulatorShader, m_Shaders.PBDCollision, m_Shaders.PBDForceVolume, m_Shaders.PBDSkinningShader, m_Shaders.PBDMeshShader, m_Shaders.PBDBodyAabbShader, m_Shaders.CameraCullingCS);
		m_DebugPass = new DebugPass(RenderPassEvent.AfterRendering, this, m_DebugMaterial);
	}

	public void DisableFeature()
	{
		Shader.SetGlobalFloat(PositionBasedDynamicsConstantBuffer._PbdEnabledGlobal, 0f);
	}

	protected override void Dispose(bool disposing)
	{
		if (m_DebugSoA != null)
		{
			m_DebugSoA.Dispose();
			m_DebugSoA = null;
		}
		if (m_Broadphase != null)
		{
			m_Broadphase.Dispose();
			m_Broadphase = null;
		}
		if (m_DebugMaterial != null)
		{
			CoreUtils.Destroy(m_DebugMaterial);
			m_DebugMaterial = null;
		}
	}
}
