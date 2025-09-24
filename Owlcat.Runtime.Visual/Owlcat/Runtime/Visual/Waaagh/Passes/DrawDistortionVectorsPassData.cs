using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawDistortionVectorsPassData : DrawRendererListPassData
{
	public TextureHandle DistortionRT;

	public TextureHandle CameraColorPyramidRT;

	public TextureHandle CameraColorRT;

	public TextureHandle CameraDepthRT;

	public Material ApplyDistortionMaterial;

	public Color ClearColor = new Color(0f, 0f, 0f, 0f);
}
