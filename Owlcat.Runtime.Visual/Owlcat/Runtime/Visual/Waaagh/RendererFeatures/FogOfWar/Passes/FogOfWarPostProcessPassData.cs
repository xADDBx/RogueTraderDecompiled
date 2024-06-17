using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar.Passes;

public class FogOfWarPostProcessPassData : PassDataBase
{
	public Material FowMaterial;

	public int ShaderPass;

	public TextureHandle CameraColorRT;

	public FogOfWarFeature Feature;

	public FogOfWarSettings Settings;

	public FogOfWarArea Area;
}
