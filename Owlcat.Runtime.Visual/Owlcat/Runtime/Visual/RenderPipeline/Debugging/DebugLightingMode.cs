using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Debugging;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@0.1.205-hotfix.1\\Runtime\\RenderPipeline\\Debugging\\DebugData.cs")]
public enum DebugLightingMode
{
	None,
	VisualizeCascade,
	BakedGI,
	Shadowmask,
	ShadowmaskRaw,
	LightAttenuation,
	Diffuse,
	Specular,
	ReflectionProbes,
	Emission
}
