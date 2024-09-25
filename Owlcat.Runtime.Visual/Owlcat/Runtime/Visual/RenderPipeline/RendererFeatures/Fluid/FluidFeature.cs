using System;
using System.Linq;
using Owlcat.Runtime.Visual.RenderPipeline.Data;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Fluid.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Fluid;

[CreateAssetMenu(menuName = "Renderer Features/Fluid")]
public class FluidFeature : ScriptableRendererFeature
{
	[Serializable]
	[ReloadGroup]
	public class ShaderResources
	{
		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/Fluid/Shaders/Fluid.shader", ReloadAttribute.Package.Root)]
		public Shader FluidShader;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/Shaders/Utils/Blit.shader", ReloadAttribute.Package.Root)]
		public Shader BlitShader;
	}

	[Serializable]
	public class FluidDebugSettings
	{
		public bool Enabled;

		public bool ForceTickEveryFrame = true;

		[Range(128f, 2048f)]
		public int TextureSize = 256;

		[Space]
		public Vector2 VelocityColorScaleOffset;

		public Vector2 DivergenceColorScaleOffset;

		public Vector2 PressureColorScaleOffset;

		public static FluidDebugSettings DefaultSettings => new FluidDebugSettings
		{
			Enabled = false,
			ForceTickEveryFrame = true,
			TextureSize = 256,
			VelocityColorScaleOffset = new Vector2(1f, 0f),
			DivergenceColorScaleOffset = new Vector2(1f, 0f),
			PressureColorScaleOffset = new Vector2(1f, 0f)
		};
	}

	private static FluidFeature m_Instance;

	public ShaderResources Shaders;

	[SerializeField]
	[Range(1f, 30f)]
	private int m_Iterations = 15;

	[SerializeField]
	[Range(0.9f, 1f)]
	private float m_Decay = 1f;

	[SerializeField]
	private float m_TextureDensity = 5f;

	[SerializeField]
	private FluidDebugSettings m_DebugSettings = FluidDebugSettings.DefaultSettings;

	private FluidSimulationPass m_FluidSimulationPass;

	private FluidCullingPass m_CullingPass;

	private FluidDebugPass m_DebugPass;

	public static FluidFeature Instance
	{
		get
		{
			if (m_Instance == null)
			{
				OwlcatRenderPipelineAsset owlcatRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as OwlcatRenderPipelineAsset;
				if (owlcatRenderPipelineAsset != null && owlcatRenderPipelineAsset.ScriptableRenderer != null && owlcatRenderPipelineAsset.ScriptableRendererData != null)
				{
					m_Instance = owlcatRenderPipelineAsset.ScriptableRendererData.rendererFeatures.FirstOrDefault((ScriptableRendererFeature f) => f is FluidFeature) as FluidFeature;
				}
			}
			return m_Instance;
		}
	}

	public int Iterations
	{
		get
		{
			return m_Iterations;
		}
		set
		{
			m_Iterations = value;
		}
	}

	public float Decay
	{
		get
		{
			return m_Decay;
		}
		set
		{
			m_Decay = value;
		}
	}

	public float TextureDensity => m_TextureDensity;

	public FluidDebugSettings DebugSettings => m_DebugSettings;

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		FluidArea active = FluidArea.Active;
		CameraType cameraType = renderingData.CameraData.Camera.cameraType;
		if (cameraType != CameraType.Preview && cameraType != CameraType.Reflection && !(active == null))
		{
			m_FluidSimulationPass.Setup(this, active);
			renderer.EnqueuePass(m_FluidSimulationPass);
			m_CullingPass.Setup(active);
			renderer.EnqueuePass(m_CullingPass);
			if (DebugSettings.Enabled)
			{
				ClusteredRenderer clusteredRenderer = renderer as ClusteredRenderer;
				m_DebugPass.Setup(this, active, clusteredRenderer.GetCurrentCameraFinalColorTexture(ref renderingData), m_DebugSettings);
				renderer.EnqueuePass(m_DebugPass);
			}
		}
	}

	public override void DisableFeature()
	{
	}

	public override string GetFeatureIdentifier()
	{
		return "FluidFeature";
	}

	public override void Create()
	{
		Material fluidMaterial = CoreUtils.CreateEngineMaterial(Shaders.FluidShader);
		Material blitMaterial = CoreUtils.CreateEngineMaterial(Shaders.FluidShader);
		m_FluidSimulationPass = new FluidSimulationPass(RenderPassEvent.BeforeRendering, fluidMaterial);
		m_CullingPass = new FluidCullingPass(RenderPassEvent.AfterRendering, fluidMaterial);
		m_DebugPass = new FluidDebugPass(RenderPassEvent.AfterRendering, blitMaterial);
	}
}
