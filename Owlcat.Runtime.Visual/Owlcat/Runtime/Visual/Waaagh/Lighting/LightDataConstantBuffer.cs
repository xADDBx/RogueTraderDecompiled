using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Lighting;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Lighting\\LightDataConstantBuffer.cs", needAccessors = false, generateCBuffer = true)]
public struct LightDataConstantBuffer
{
	public const int MaxVisibleLights = 1024;

	public const int LightPackFalloffTypeOffset = 0;

	public const int LightPackFalloffTypeCount = 1;

	public const int LightPackSnapSpecularToInnerRadiusOffset = 1;

	public const int LightPackSnapSpecularToInnerRadiusCount = 1;

	public const int LightPackShadowsFlagOffset = 2;

	public const int LightPackShadowsFlagCount = 1;

	public const int LightPackShadowDataIndexOffset = 3;

	public const int LightPackShadowDataIndexCount = 7;

	public const int LightPackShadowmaskFlagOffset = 10;

	public const int LightPackShadowmaskFlagCount = 1;

	public const int LightPackShadowmaskChannelOffset = 11;

	public const int LightPackShadowmaskChannelCount = 2;

	public const int LightPackLightLayerMaskOffset = 13;

	public const int LightPackLightLayerMaskCount = 8;

	public const int LightPackVolumetricLightingFlagOffset = 21;

	public const int LightPackVolumetricLightingFlagCount = 1;

	public const int LightPackVolumetricShadowsFlagOffset = 22;

	public const int LightPackVolumetricShadowsFlagCount = 1;

	public const int LightPackLightCookieFlagOffset = 23;

	public const int LightPackLightCookieFlagCount = 1;

	public const int LightPackLightCookieIndexOffset = 24;

	public const int LightPackLightCookieIndexCount = 7;

	[HLSLArray(1024, typeof(Vector4))]
	public unsafe fixed float _LightPositionsAndPackedData[4096];

	[HLSLArray(1024, typeof(Vector4))]
	public unsafe fixed float _LightAttenuations[4096];

	[HLSLArray(1024, typeof(Vector4))]
	public unsafe fixed float _LightColorsAndShadowStrength[4096];

	[HLSLArray(1024, typeof(Vector4))]
	public unsafe fixed float _LightSpotDirectionsAndInnerRadius[4096];
}
