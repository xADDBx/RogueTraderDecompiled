using UnityEngine.Rendering;

namespace Owlcat.ShaderLibrary.Visual.Debug;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@0.1.216\\ShaderLibrary\\Debug\\DebugViewEnums.cs")]
public enum DebugClustersMode
{
	None,
	Heatmap,
	HeatmapShadowedLights,
	DeferredLightingComplexity
}
