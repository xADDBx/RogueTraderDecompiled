using System;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.OccludedObjectHighlighting.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.OccludedObjectHighlighting;

[CreateAssetMenu(menuName = "Renderer Features/Occluded Object Highlighting")]
[ReloadGroup]
public class OccludedObjectHighlightingFeature : ScriptableRendererFeature
{
	[Serializable]
	[ReloadGroup]
	public sealed class ShaderResources
	{
		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/OccludedObjectHighlighting/Shaders/OccludedObject.shader", ReloadAttribute.Package.Root)]
		public Shader OccludedObjectShader;

		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/OccludedObjectHighlighting/Shaders/BakeNoise3D.shader", ReloadAttribute.Package.Root)]
		public Shader NoiseBakeShader;
	}

	[Serializable]
	public sealed class DepthClipSettings
	{
		[Range(0f, 1f)]
		public float ClipTreshold = 0.1f;

		public float NoiseTiling = 1f;

		public float AlphaScale = 1f;

		public float NearCameraClipDistance = 3f;
	}

	private const string kBasePath = "Assets/Code/Owlcat/";

	[SerializeField]
	private ShaderResources m_Shaders = new ShaderResources();

	[SerializeField]
	[Reload("Runtime/RenderPipeline/RendererFeatures/OccludedObjectHighlighting/OccludedObjectWithInstancing.mat", ReloadAttribute.Package.Root)]
	private Material m_OccludedObjectMaterialWithInstancing;

	[SerializeField]
	private DepthClipSettings m_DepthClipSettings = new DepthClipSettings();

	private RenderTexture m_Noise3D;

	private OccludedObjectNoiseBakePass m_NoiseBakePass;

	private OccludedObjectDepthClipperPass m_DephtClipperPass;

	private float m_PrevNoiseTiling;

	public ShaderResources Shaders => m_Shaders;

	public DepthClipSettings DepthClip => m_DepthClipSettings;

	public RenderTexture Noise3D => m_Noise3D;

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (renderingData.CameraData.Camera.cameraType != CameraType.Game || m_DepthClipSettings.NearCameraClipDistance <= 0f)
		{
			DisableFeature();
			return;
		}
		if (m_Noise3D == null)
		{
			RenderTextureDescriptor desc = new RenderTextureDescriptor(32, 32, RenderTextureFormat.R8);
			desc.depthBufferBits = 0;
			desc.dimension = TextureDimension.Tex3D;
			desc.volumeDepth = 32;
			m_Noise3D = new RenderTexture(desc);
			m_Noise3D.name = "Occluded Object Noise Map 3D";
			m_Noise3D.filterMode = FilterMode.Trilinear;
			m_NoiseBakePass.Setup(this);
			renderer.EnqueuePass(m_NoiseBakePass);
			m_PrevNoiseTiling = m_DepthClipSettings.NoiseTiling;
		}
		if (m_DepthClipSettings.NoiseTiling != m_PrevNoiseTiling)
		{
			m_NoiseBakePass.Setup(this);
			renderer.EnqueuePass(m_NoiseBakePass);
			m_PrevNoiseTiling = m_DepthClipSettings.NoiseTiling;
		}
		m_DephtClipperPass.Setup(this);
		renderer.EnqueuePass(m_DephtClipperPass);
		Shader.SetGlobalFloat(CameraBuffer._OccludedObjectHighlightingFeatureEnabled, 1f);
	}

	public override void Create()
	{
		Material material = CoreUtils.CreateEngineMaterial(m_Shaders.OccludedObjectShader);
		material.enableInstancing = true;
		Material material2 = CoreUtils.CreateEngineMaterial(m_Shaders.NoiseBakeShader);
		m_NoiseBakePass = new OccludedObjectNoiseBakePass(RenderPassEvent.BeforeRendering, material2);
		m_DephtClipperPass = new OccludedObjectDepthClipperPass(RenderPassEvent.BeforeRenderingPrepasses, material);
	}

	public override void DisableFeature()
	{
		Shader.SetGlobalFloat(CameraBuffer._OccludedObjectHighlightingFeatureEnabled, 0f);
	}

	protected override void Dispose(bool disposing)
	{
		if (m_Noise3D != null)
		{
			m_Noise3D.Release();
			m_Noise3D = null;
		}
	}

	public override string GetFeatureIdentifier()
	{
		return "OccludedObjecHighlightingFeature";
	}
}
