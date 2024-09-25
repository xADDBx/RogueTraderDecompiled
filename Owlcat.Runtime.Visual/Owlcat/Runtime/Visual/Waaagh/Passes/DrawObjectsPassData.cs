using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawObjectsPassData : DrawRendererListPassData
{
	public TextureHandle CameraNormalsRT;

	public TextureHandle CameraBakedGIRT;

	public TextureHandle CameraShadowmaskRT;

	public TextureHandle CameraDepthCopyRT;

	public RenderQueueRange RenderQueueRange;

	public CameraType CameraType;

	public bool IsIndirectRenderingEnabled;

	public bool IsSceneViewInPrefabEditMode;
}
