using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar.Passes;

public class FogOfWarShadowmapPassData : PassDataBase
{
	public TextureHandle FowHistoryCopyRT;

	public TextureHandle FowBlur0RT;

	public TextureHandle FowBlur1RT;

	public Matrix4x4 Proj;

	public Matrix4x4 View;

	public FogOfWarArea Area;

	public FogOfWarSettings Settings;

	public FogOfWarFeature Feature;

	public Material FowMaterial;

	public Material BlurMaterial;

	public int FowClearPass;

	public int FowDrawShadowsPass;

	public int FowDrawCharacterQuadPass;

	public int FowDrawCharacterQuadMaskPass;

	public int FowFinalBlendPass;

	public int FowFinalBlendAndMaskPass;

	public int FowHistoryCopyPass;
}
