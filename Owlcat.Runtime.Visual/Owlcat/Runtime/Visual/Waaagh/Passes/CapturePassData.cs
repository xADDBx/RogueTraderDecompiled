using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class CapturePassData : PassDataBase
{
	public IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> CaptureActions;

	public TextureHandle CameraColorBuffer;
}
