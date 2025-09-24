using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public sealed class ClearGBufferPassData : PassDataBase
{
	public TextureHandle CameraAlbedoRT;

	public TextureHandle CameraSpecularRT;

	public TextureHandle CameraNormalsRT;

	public TextureHandle CameraBakedGIRT;

	public TextureHandle CameraShadowmaskRT;

	public TextureHandle CameraTranslucencyRT;

	public RTClearFlags ClearFlags;
}
