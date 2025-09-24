using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class DrawObjectsWireframePassData : DrawRendererListPassData
{
	public TextureHandle RenderTarget;

	public Color ClearColor;
}
