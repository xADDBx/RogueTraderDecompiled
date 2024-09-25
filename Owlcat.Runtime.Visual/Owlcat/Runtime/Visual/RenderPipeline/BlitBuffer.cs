using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public static class BlitBuffer
{
	public static int _BlitTexture = Shader.PropertyToID("_BlitTexture");

	public static int _BlitScaleBias = Shader.PropertyToID("_BlitScaleBias");

	public static int _BlitScaleBiasRt = Shader.PropertyToID("_BlitScaleBiasRt");

	public static int _BlitMipLevel = Shader.PropertyToID("_BlitMipLevel");
}
