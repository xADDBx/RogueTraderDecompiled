using Owlcat.Runtime.Visual.Waaagh.Debugging;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class FullscreenDebugPassData : PassDataBase
{
	public WaaaghDebugData DebugData;

	public Material Material;

	public TextureHandle CameraDepthRT;

	public TextureHandle CameraFinalTarget;

	public TextureHandle TempTarget;

	public ComputeBufferHandle FullScreenDebugBuffer;
}
