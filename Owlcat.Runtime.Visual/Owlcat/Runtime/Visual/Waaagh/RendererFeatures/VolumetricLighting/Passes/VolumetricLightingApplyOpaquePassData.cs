using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;

public class VolumetricLightingApplyOpaquePassData : PassDataBase
{
	public Material Material;

	public TextureHandle ScatterTexture;

	public TextureHandle CameraColorBuffer;

	public TextureHandle DepthCopyTexture;
}
