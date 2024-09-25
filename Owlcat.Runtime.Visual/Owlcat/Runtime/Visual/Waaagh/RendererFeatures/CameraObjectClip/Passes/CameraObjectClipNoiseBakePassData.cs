using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CameraObjectClip.Passes;

public class CameraObjectClipNoiseBakePassData : PassDataBase
{
	public Material NoiseBakeMaterial;

	public TextureHandle Noise3DRT;

	public int VolumeDepth;

	public float NoiseTiling;
}
