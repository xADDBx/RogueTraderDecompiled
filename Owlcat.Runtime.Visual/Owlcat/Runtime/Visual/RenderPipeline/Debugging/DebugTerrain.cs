using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Debugging;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@0.1.216\\Runtime\\RenderPipeline\\Debugging\\DebugData.cs")]
public enum DebugTerrain
{
	None,
	Weights,
	Indices,
	TriplanarProjections
}
