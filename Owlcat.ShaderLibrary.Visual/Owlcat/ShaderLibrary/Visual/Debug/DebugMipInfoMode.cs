using UnityEngine.Rendering;

namespace Owlcat.ShaderLibrary.Visual.Debug;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\ShaderLibrary\\Debug\\DebugViewEnums.cs")]
public enum DebugMipInfoMode
{
	None,
	Level,
	Count,
	Ratio
}
