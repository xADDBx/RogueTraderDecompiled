using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public static class CommonTextureId
{
	public static int Unity_CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");

	public static int _ShadowmapRT = Shader.PropertyToID("_ShadowmapRT");

	public static int _ScreenSpaceShadowmapUAV = Shader.PropertyToID("_ScreenSpaceShadowmapUAV");

	public static int _ReflectionProbeArray = Shader.PropertyToID("_ReflectionProbeArray");
}
