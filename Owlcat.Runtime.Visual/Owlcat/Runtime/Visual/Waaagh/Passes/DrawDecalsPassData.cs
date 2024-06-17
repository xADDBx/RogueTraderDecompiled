using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawDecalsPassData : DrawRendererListPassData
{
	public Material DBufferBlitMaterial;

	public bool DrawGUIDecals;

	public TextureHandle CameraColorRT;

	public TextureHandle CameraDepthRT;

	public TextureHandle CameraDepthCopyRT;

	public TextureHandle CameraAlbedoRT;

	public TextureHandle CameraSpecularRT;

	public TextureHandle CameraNormalsRT;

	public TextureHandle CameraEmissionRT;

	public TextureHandle DBuffer0RT;

	public TextureHandle DBuffer1RT;

	public DrawingSettings DrawingSettings;

	public CullingResults CullingResults;

	public FilteringSettings FilteringSettings;
}
