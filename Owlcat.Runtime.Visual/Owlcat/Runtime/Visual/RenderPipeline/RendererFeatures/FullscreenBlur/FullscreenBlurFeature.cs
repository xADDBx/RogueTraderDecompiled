using System;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FullscreenBlur.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FullscreenBlur;

[CreateAssetMenu(menuName = "Renderer Features/Fullscreen Blur")]
public class FullscreenBlurFeature : ScriptableRendererFeature
{
	[Serializable]
	[ReloadGroup]
	public sealed class ShaderResources
	{
		[SerializeField]
		[Reload("Runtime/RenderPipeline/Shaders/Utils/MobileBlur.shader", ReloadAttribute.Package.Root)]
		public Shader BlurShader;
	}

	public ShaderResources Shaders;

	public RenderPassEvent Event;

	public int EventOffset;

	public BlurType BlurType;

	public Downsample Downsample = Downsample.Downsample2x2;

	[Range(0f, 10f)]
	public float BlurSize;

	[Range(1f, 4f)]
	public int BlurIterations = 1;

	private FullscreenBlurPass m_Pass;

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (renderer is ClusteredRenderer clusteredRenderer && BlurSize > 0f)
		{
			m_Pass.Setup(this, clusteredRenderer.GetCurrentCameraColorTexture());
			renderer.EnqueuePass(m_Pass);
		}
	}

	public override void Create()
	{
		Material material = CoreUtils.CreateEngineMaterial(Shaders.BlurShader);
		m_Pass = new FullscreenBlurPass(Event + EventOffset, material);
	}

	public override void DisableFeature()
	{
	}

	public override string GetFeatureIdentifier()
	{
		return "FullscreenBlurFeature";
	}
}
