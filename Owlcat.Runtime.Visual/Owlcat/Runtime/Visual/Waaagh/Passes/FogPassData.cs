using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class FogPassData : PassDataBase
{
	public Material Material;

	public TextureHandle CameraColorRT;

	public TextureHandle CameraDepthCopyRT;
}
