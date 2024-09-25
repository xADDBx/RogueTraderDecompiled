using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public static class ShadowBuffer
{
	public static int _FaceVectors = Shader.PropertyToID("_FaceVectors");

	public static int _ShadowFadeDistanceScaleAndBias = Shader.PropertyToID("_ShadowFadeDistanceScaleAndBias");

	public static int _ShadowEntryIndex = Shader.PropertyToID("_ShadowEntryIndex");

	public static int _OffsetFactor = Shader.PropertyToID("_OffsetFactor");

	public static int _OffsetUnits = Shader.PropertyToID("_OffsetUnits");

	public static int _ShadowFaceCount = Shader.PropertyToID("_ShadowFaceCount");

	public static int _Clips = Shader.PropertyToID("_Clips");

	public static int _LightDirection = Shader.PropertyToID("_LightDirection");

	public static int _PunctualNearClip = Shader.PropertyToID("_PunctualNearClip");

	public static int _FaceId = Shader.PropertyToID("_FaceId");

	public static int _ScreenSpaceShadowmapUAV_Size = Shader.PropertyToID("_ScreenSpaceShadowmapUAV_Size");

	public static int _ZClip = Shader.PropertyToID("_ZClip");
}
