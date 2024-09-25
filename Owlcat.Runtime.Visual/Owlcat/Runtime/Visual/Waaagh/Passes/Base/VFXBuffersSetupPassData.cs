using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class VFXBuffersSetupPassData : PassDataBase
{
	public Camera Camera;

	public TextureHandle CameraHistoryDepthRT;

	public TextureHandle CameraHistoryColorRT;

	public TextureHandle CameraColorRT;

	public int2 ScreenSize;

	public bool NeedDepth;

	public bool NeedColor;

	public bool NeedColorBlit;
}
