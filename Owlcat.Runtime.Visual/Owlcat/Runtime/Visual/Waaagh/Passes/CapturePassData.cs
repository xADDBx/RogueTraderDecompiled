using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class CapturePassData : PassDataBase
{
	public IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> CaptureActions;

	public TextureHandle CameraColorBuffer;
}
