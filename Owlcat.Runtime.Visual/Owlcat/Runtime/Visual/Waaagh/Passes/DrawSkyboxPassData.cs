using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawSkyboxPassData : PassDataBase
{
	public TextureHandle ColorOutput;

	public TextureHandle DepthOutput;

	public Camera Camera;
}
