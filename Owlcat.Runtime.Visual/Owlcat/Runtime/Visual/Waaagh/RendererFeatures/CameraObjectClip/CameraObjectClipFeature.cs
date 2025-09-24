using System;
using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CameraObjectClip.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CameraObjectClip;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Camera Object Clip")]
[ReloadGroup]
public class CameraObjectClipFeature : ScriptableRendererFeature
{
	[Serializable]
	[ReloadGroup]
	public sealed class ShaderResources
	{
		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/CameraObjectClip/Shaders/BakeNoise3D.shader", ReloadAttribute.Package.Root)]
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

		public bool OcclusionGeometryClipEnabled;

		public Settings OcclusionGeometryClipSettings = Owlcat.Runtime.Visual.OcclusionGeometryClip.Settings.Default;
	}

	public ShaderResources Shaders;

	[SerializeField]
	private DepthClipSettings m_DepthClipSettings = new DepthClipSettings();

	private CameraObjectClipNoiseBakePass m_NoiseBakePass;

	private CameraObjectClipSetupPass m_SetupPass;

	private Material m_NoiseBakeMaterial;

	private float m_PrevNoiseTiling;

	private RTHandle m_Noise3D;

	public DepthClipSettings Settings => m_DepthClipSettings;

	public RTHandle Noise3D => m_Noise3D;

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (renderingData.CameraData.Camera.cameraType != CameraType.Game)
		{
			DisableFeature();
			return;
		}
		if (m_PrevNoiseTiling != m_DepthClipSettings.NoiseTiling)
		{
			renderer.EnqueuePass(m_NoiseBakePass);
			m_PrevNoiseTiling = m_DepthClipSettings.NoiseTiling;
		}
		renderer.EnqueuePass(m_SetupPass);
	}

	public override void Create()
	{
		m_NoiseBakeMaterial = CoreUtils.CreateEngineMaterial(Shaders.NoiseBakeShader);
		m_Noise3D = RTHandles.Alloc(32, 32, 32, DepthBits.None, GraphicsFormat.R8_UNorm, FilterMode.Trilinear, TextureWrapMode.Repeat, TextureDimension.Tex3D, enableRandomWrite: false, useMipMap: false, autoGenerateMips: true, isShadowMap: false, 1, 0f, MSAASamples.None, bindTextureMS: false, useDynamicScale: false, useDynamicScaleExplicit: false, RenderTextureMemoryless.None, VRTextureUsage.None, "CameraObjectClipNoise3D");
		m_PrevNoiseTiling = -1f;
		m_NoiseBakePass = new CameraObjectClipNoiseBakePass(RenderPassEvent.BeforeRendering, this, m_NoiseBakeMaterial);
		m_SetupPass = new CameraObjectClipSetupPass(RenderPassEvent.BeforeRendering, this);
		Owlcat.Runtime.Visual.OcclusionGeometryClip.System.SetEnabled(Settings.OcclusionGeometryClipEnabled);
		Owlcat.Runtime.Visual.OcclusionGeometryClip.System.SetSettings(Settings.OcclusionGeometryClipSettings);
	}

	public void DisableFeature()
	{
		Shader.SetGlobalFloat(ShaderPropertyId._OccludedObjectHighlightingFeatureEnabled, 0f);
	}

	protected override void Dispose(bool disposing)
	{
		CoreUtils.Destroy(m_NoiseBakeMaterial);
		if (m_Noise3D != null)
		{
			RTHandles.Release(m_Noise3D);
		}
	}
}
