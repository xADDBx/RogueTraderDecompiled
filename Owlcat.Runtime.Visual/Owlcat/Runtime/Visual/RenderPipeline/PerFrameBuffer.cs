using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public static class PerFrameBuffer
{
	public static int _GlossyEnvironmentColor = Shader.PropertyToID("_GlossyEnvironmentColor");

	public static int _HexRatio = Shader.PropertyToID("_HexRatio");

	public static int _Time = Shader.PropertyToID("_Time");

	public static int _SinTime = Shader.PropertyToID("_SinTime");

	public static int _CosTime = Shader.PropertyToID("_CosTime");

	public static int unity_DeltaTime = Shader.PropertyToID("unity_DeltaTime");

	public static int _UnscaledTime = Shader.PropertyToID("_UnscaledTime");
}
