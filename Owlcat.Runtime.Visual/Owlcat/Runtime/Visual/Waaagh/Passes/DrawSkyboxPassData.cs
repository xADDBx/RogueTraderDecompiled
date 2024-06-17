using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawSkyboxPassData : PassDataBase
{
	public TextureHandle ColorOutput;

	public TextureHandle DepthOutput;

	public Camera Camera;
}
