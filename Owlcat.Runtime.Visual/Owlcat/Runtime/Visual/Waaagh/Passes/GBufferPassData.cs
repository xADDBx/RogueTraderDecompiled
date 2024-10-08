using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class GBufferPassData : DrawRendererListPassData
{
	public TextureHandle CameraAlbedoRT;

	public TextureHandle CameraSpecularRT;

	public TextureHandle CameraNormalsRT;

	public TextureHandle CameraEmissionRT;

	public TextureHandle CameraBakedGIRT;

	public TextureHandle CameraShadowmaskRT;

	public TextureHandle CameraTranslucencyRT;

	public TextureHandle CameraDepthBuffer;

	public RenderQueueRange RenderQueueRange;

	public CameraType CameraType;

	public bool IsIndirectRenderingEnabled;

	public bool IsSceneViewInPrefabEditMode;
}
