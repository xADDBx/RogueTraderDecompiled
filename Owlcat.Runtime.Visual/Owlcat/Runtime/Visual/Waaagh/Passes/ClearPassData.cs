using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class ClearPassData : PassDataBase
{
	public TextureHandle CameraColorBuffer;

	public TextureHandle CameraDepthBuffer;

	public TextureHandle CameraDepthCopy;

	public Color ClearColor;

	public Color DepthCopyClearColor;

	public RTClearFlags ClearFlags;
}
