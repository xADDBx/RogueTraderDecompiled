using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public static class ScreenSpaceReflectionsBuffer
{
	public static int _SsrIterLimit = Shader.PropertyToID("_SsrIterLimit");

	public static int _SsrThicknessScale = Shader.PropertyToID("_SsrThicknessScale");

	public static int _SsrThicknessBias = Shader.PropertyToID("_SsrThicknessBias");

	public static int _SsrFresnelPower = Shader.PropertyToID("_SsrFresnelPower");

	public static int _SsrConfidenceScale = Shader.PropertyToID("_SsrConfidenceScale");

	public static int _SsrMinDepthMipLevel = Shader.PropertyToID("_SsrMinDepthMipLevel");
}
