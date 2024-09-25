using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.OccludedObjectHighlighting.Passes;

internal class OccludedObjectHighlighterPassData : PassDataBase
{
	public float Intensity;

	public Material HighlighterMaterial;

	public Material BlurMaterial;

	public Material CompositeMaterial;

	public Shader ParticlesShader;

	public OccludedObjectHighlightingFeature Feature;

	public TextureHandle CameraColorBuffer;

	public TextureHandle CameraDepthBuffer;

	public TextureHandle HighlightRT;

	public TextureHandle Blur1RT;

	public TextureHandle Blur2RT;

	public OccludedObjectHighlightingFeature.BlurDirections BlurDirections;

	public int BlurIterations;

	public float BlurMinSpread;

	public float BlurSpread;

	public Vector4 CompositeParams;
}
