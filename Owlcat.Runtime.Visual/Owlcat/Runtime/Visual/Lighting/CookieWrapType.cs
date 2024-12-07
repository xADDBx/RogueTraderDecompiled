using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Lighting;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@1.1.230\\Runtime\\Lighting\\LightCookieConstantBuffer.cs", needAccessors = false)]
public enum CookieWrapType
{
	Clamp,
	Repeat
}
