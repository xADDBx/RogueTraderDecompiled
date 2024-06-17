using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class DrawObjectsWireframePassData : DrawRendererListPassData
{
	public TextureHandle RenderTarget;

	public Color ClearColor;
}
