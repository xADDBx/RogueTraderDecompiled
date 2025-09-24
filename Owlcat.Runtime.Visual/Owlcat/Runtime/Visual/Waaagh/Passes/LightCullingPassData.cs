using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class LightCullingPassData : PassDataBase
{
	public ComputeShader LightCullingShader;

	public int BuildLightTilesKernel;

	public BufferHandle LightTilesBuffer;

	public int3 DispatchSize;

	public Matrix4x4 ScreenProjMatrix;

	public int LightTilesBufferSize;

	public TextureHandle TilesMinMaxZTexture;
}
