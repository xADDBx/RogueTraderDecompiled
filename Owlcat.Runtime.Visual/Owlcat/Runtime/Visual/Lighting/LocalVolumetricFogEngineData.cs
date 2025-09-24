using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Lighting;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Lighting\\LocalVolumetricFogGpuData.cs")]
public struct LocalVolumetricFogEngineData
{
	public Vector3 scattering;

	public float extinction;

	public Vector3 textureTiling;

	public int invertFade;

	public Vector3 textureScroll;

	public float rcpDistFadeLen;

	public Vector3 rcpPosFaceFade;

	public float endTimesRcpDistFadeLen;

	public Vector3 rcpNegFaceFade;

	public int useVolumeMask;

	public Vector3 atlasOffset;

	public LocalVolumetricFogFalloffMode falloffMode;

	public Vector4 maskSize;

	public static LocalVolumetricFogEngineData GetNeutralValues()
	{
		LocalVolumetricFogEngineData result = default(LocalVolumetricFogEngineData);
		result.scattering = Vector3.zero;
		result.extinction = 0f;
		result.atlasOffset = Vector3.zero;
		result.textureTiling = Vector3.one;
		result.textureScroll = Vector3.zero;
		result.rcpPosFaceFade = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		result.rcpNegFaceFade = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		result.invertFade = 0;
		result.rcpDistFadeLen = 0f;
		result.endTimesRcpDistFadeLen = 1f;
		result.useVolumeMask = 0;
		result.maskSize = Vector4.zero;
		result.falloffMode = LocalVolumetricFogFalloffMode.Linear;
		return result;
	}
}
