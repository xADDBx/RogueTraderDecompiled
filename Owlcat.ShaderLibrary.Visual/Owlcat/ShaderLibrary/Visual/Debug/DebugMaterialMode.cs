using UnityEngine.Rendering;

namespace Owlcat.ShaderLibrary.Visual.Debug;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@94246ccf1d50\\ShaderLibrary\\Debug\\DebugViewEnums.cs")]
public enum DebugMaterialMode
{
	None,
	Albedo,
	Specular,
	Alpha,
	Smoothness,
	NormalWorldSpace,
	LightingComplexity,
	Metallic
}
