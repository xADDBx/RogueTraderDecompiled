using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Lighting;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@94246ccf1d50\\Runtime\\Lighting\\LightCookieConstantBuffer.cs", needAccessors = false, generateCBuffer = true)]
public struct LightCookieConstantBuffer
{
	public const int MaxLightCookieCount = 128;

	[HLSLArray(128, typeof(Matrix4x4))]
	public unsafe fixed float _LightCookieMatrices[2048];

	[HLSLArray(128, typeof(Vector4))]
	public unsafe fixed float _LightCookieUVRects[512];
}
