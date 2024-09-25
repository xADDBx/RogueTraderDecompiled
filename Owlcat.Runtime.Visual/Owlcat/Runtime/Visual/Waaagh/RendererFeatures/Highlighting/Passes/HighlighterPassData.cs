using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting.Passes;

public class HighlighterPassData : PassDataBase
{
	public TextureHandle CameraColorRT;

	public TextureHandle DepthBuffer;

	public HighlightingFeature.ZTestMode ZTestMode;

	public TextureHandle HighlightRT;

	public TextureHandle Blur1RT;

	public TextureHandle Blur2RT;

	public HighlightingFeature Feature;

	public Material HighlighterMaterial;

	public Material BlurMaterial;

	public Material CutMaterial;

	public Material CompositeMaterial;

	public Shader ParticlesShader;
}
