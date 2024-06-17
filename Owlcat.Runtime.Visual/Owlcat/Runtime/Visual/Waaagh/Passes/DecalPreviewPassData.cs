using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DecalPreviewPassData : DrawRendererListPassData
{
	public TextureHandle CameraDepthRT;

	public TextureHandle CameraColorRT;
}
