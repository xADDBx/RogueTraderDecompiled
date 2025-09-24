using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Debugging;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\RenderPipeline\\Debugging\\DebugData.cs")]
public enum DebugMaterial
{
	None,
	Albedo,
	Roughness,
	Metallic,
	Emission,
	Translucency,
	OccludedObjectClip
}
