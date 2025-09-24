using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class ComputeTilesMinMaxZPassData : PassDataBase
{
	public ComputeShader Shader;

	public TextureHandle CameraDepthCopyTexture;

	public TextureHandle TilesMinMaxZTexture;

	public int3 DispatchSize;

	public int TileSize;

	public bool MaxDepthOnly;
}
