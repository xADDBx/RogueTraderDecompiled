using Owlcat.Runtime.Visual.Overrides.HBAO;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class HbaoPassData : PassDataBase
{
	public Material Material;

	public Hbao Settings;

	public TextureHandle CameraDepthRT;

	public TextureHandle CameraNormalsRT;

	public TextureHandle CameraColorRT;

	public TextureHandle HbaoRT;

	public TextureHandle HbaoBlurRT;

	public TextureHandle HbaoRT3;
}
