using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Lighting;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@0.1.216\\Runtime\\Lighting\\LightCookieConstantBuffer.cs", needAccessors = false)]
public enum LightCookieShaderFormat
{
	None = -1,
	RGB,
	Alpha,
	Red
}
