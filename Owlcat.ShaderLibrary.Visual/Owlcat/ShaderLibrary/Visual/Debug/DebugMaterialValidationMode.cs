using UnityEngine.Rendering;

namespace Owlcat.ShaderLibrary.Visual.Debug;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@1.1.230\\ShaderLibrary\\Debug\\DebugViewEnums.cs")]
public enum DebugMaterialValidationMode
{
	None,
	Albedo,
	Metallic
}
