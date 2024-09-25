using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Lighting;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@1.1.225\\Runtime\\Lighting\\LightCookieConstantBuffer.cs", needAccessors = false)]
public enum CookieType
{
	Directional,
	Spot,
	Point
}
