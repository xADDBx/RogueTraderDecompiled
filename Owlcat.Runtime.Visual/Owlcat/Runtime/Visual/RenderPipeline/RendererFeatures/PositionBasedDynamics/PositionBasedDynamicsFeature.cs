using System;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Broadphase;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Debugging;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics;

[CreateAssetMenu(menuName = "Renderer Features/Position Based Dynamics")]
public class PositionBasedDynamicsFeature : ScriptableRendererFeature
{
	[Serializable]
	[ReloadGroup]
	public class ShaderResources
	{
		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDSingleDispatchSimulator.compute", ReloadAttribute.Package.Root)]
		public ComputeShader PBDSingleDispatchSimulatorShader;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDCollision.compute", ReloadAttribute.Package.Root)]
		public ComputeShader PBDCollision;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDForceVolume.compute", ReloadAttribute.Package.Root)]
		public ComputeShader PBDForceVolume;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDSkinning.compute", ReloadAttribute.Package.Root)]
		public ComputeShader PBDSkinningShader;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDMesh.compute", ReloadAttribute.Package.Root)]
		public ComputeShader PBDMeshShader;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDBodyAabb.compute", ReloadAttribute.Package.Root)]
		public ComputeShader PBDBodyAabbShader;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDDebug.shader", ReloadAttribute.Package.Root)]
		public Shader PBDDebug;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/GPUParallelSort/RadixSort.compute", ReloadAttribute.Package.Root)]
		public ComputeShader RadixSortCS;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/GPUHashtable/LinearProbingHashtable.compute", ReloadAttribute.Package.Root)]
		public ComputeShader HashtableCS;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/OptimizedSpatialHashingBroadphase.compute", ReloadAttribute.Package.Root)]
		public ComputeShader SpatialHashingCS;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/PositionBasedDynamics/Shaders/PBDCameraCulling.compute", ReloadAttribute.Package.Root)]
		public ComputeShader CameraCullingCS;
	}

	[Serializable]
	public class PBDDebugSettings
	{
		public bool Enabled;

		public float ParticleSize = 0.1f;

		public Color ParticleColor = Color.red;

		public Color ConstraintColor = Color.yellow;

		public bool ShowNormals;

		public Color NormalsColor = Color.blue;

		public bool ShowCollidersAabb;

		public bool ShowCollidersGrid;

		public bool ShowForceVolumesAabb;

		public bool ShowForceVolumesGrid;
	}

	[SerializeField]
	private ShaderResources m_Shaders;

	[SerializeField]
	private PBDDebugSettings m_DebugSettings;

	private PositionBasedDynamicsConfig m_Config;

	private Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Passes.DebugPass m_DebugPass;

	private SingleDispatchSimulationPass m_SingleDispatchSimulationPass;

	private GPUBroadphaseBase m_Broadphase;

	private GPUDebugSoA m_DebugSoA;

	public PBDDebugSettings DebugSettings => m_DebugSettings;

	internal GPUBroadphaseBase Broadphase => m_Broadphase;

	internal GPUDebugSoA DebugSoA => m_DebugSoA;

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
		if (!m_Config.GPU || PBD.IsEmpty)
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
		ClusteredRenderer clusteredRenderer = renderer as ClusteredRenderer;
		m_SingleDispatchSimulationPass.Setup(m_Broadphase, m_Config.SimulationIterations, m_Config.ConstraintIterations, m_Config.Decay);
		renderer.EnqueuePass(m_SingleDispatchSimulationPass);
		if (m_DebugSettings.Enabled && !PBD.IsSceneInitialization)
		{
			if (m_DebugSoA == null)
			{
				m_DebugSoA = new GPUDebugSoA();
			}
			m_DebugPass.Setup(this, clusteredRenderer.GetCurrentCameraFinalColorTexture(ref renderingData));
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
		Material debugMaterial = CoreUtils.CreateEngineMaterial(m_Shaders.PBDDebug);
		m_SingleDispatchSimulationPass = new SingleDispatchSimulationPass((RenderPassEvent)1, m_Shaders.PBDSingleDispatchSimulatorShader, m_Shaders.PBDCollision, m_Shaders.PBDForceVolume, m_Shaders.PBDSkinningShader, m_Shaders.PBDMeshShader, m_Shaders.PBDBodyAabbShader, m_Shaders.CameraCullingCS);
		m_DebugPass = new Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Passes.DebugPass(RenderPassEvent.AfterRendering, debugMaterial);
	}

	public override void DisableFeature()
	{
		Shader.SetGlobalFloat(PositionBasedDynamicsConstantBuffer._PbdEnabledGlobal, 0f);
	}

	public override string GetFeatureIdentifier()
	{
		return "Position Based Dynamics Feature";
	}

	protected override void Dispose(bool disposing)
	{
		if (m_DebugSoA != null)
		{
			m_DebugSoA.Dispose();
		}
		if (m_Broadphase != null)
		{
			m_Broadphase.Dispose();
		}
	}
}
