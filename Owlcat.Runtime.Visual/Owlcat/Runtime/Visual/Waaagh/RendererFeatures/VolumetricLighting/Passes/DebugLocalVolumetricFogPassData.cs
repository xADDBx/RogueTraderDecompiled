using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;

public class DebugLocalVolumetricFogPassData : PassDataBase
{
	public Material Material;

	public TextureHandle CameraColorBuffer;

	public TextureHandle DepthCopyTexture;

	public Vector4 LocalFogClusteringParams;

	public BufferHandle FogTilesBuffer;

	public BufferHandle FogZBinsBuffer;
}
