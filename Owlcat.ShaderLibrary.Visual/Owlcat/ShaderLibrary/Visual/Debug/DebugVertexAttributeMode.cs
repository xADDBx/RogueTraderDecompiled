using UnityEngine.Rendering;

namespace Owlcat.ShaderLibrary.Visual.Debug;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@0.1.205-hotfix.1\\ShaderLibrary\\Debug\\DebugViewEnums.cs")]
public enum DebugVertexAttributeMode
{
	None,
	Texcoord0,
	Texcoord1,
	Texcoord2,
	Texcoord3,
	Color,
	Tangent,
	Normal
}
