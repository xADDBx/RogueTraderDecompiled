using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FullscreenBlur.Passes;

public class FullscreenBlurPassData : PassDataBase
{
	public FullscreenBlurFeature Feature;

	public Material BlurMaterial;

	public TextureHandle CameraColorRT;

	public TextureHandle BlurRT0;

	public TextureHandle BlurRT1;
}
