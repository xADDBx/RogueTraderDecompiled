using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.ShaderLibrary.Visual.Debug;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@1.1.225\\ShaderLibrary\\Debug\\DebugViewEnums.cs")]
public enum DebugValidationMode
{
	None,
	[InspectorName("Highlight NaN, Inf and Negative Values")]
	HighlightNanInfNegative,
	[InspectorName("Highlight Values Outside Range")]
	HighlightOutsideOfRange
}
