using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class CopyDepthPassData : PassDataBase
{
	public TextureHandle Input;

	public TextureHandle Output;

	public Material Material;

	public int ShaderPass;

	public bool SetGlobalTexture;

	public Vector4 DepthPyramidSamplingRatio;

	public RendererListHandle RendererList;

	public CopyDepthPass.PassCullingCriteria CullingCriteria;
}
