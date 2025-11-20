using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Debugging;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@94246ccf1d50\\Runtime\\RenderPipeline\\Debugging\\DebugData.cs")]
public enum DebugBuffers
{
	None,
	Depth,
	GBuffer0_RGB,
	GBuffer0_A,
	GBuffer1_RGB,
	GBuffer1_A,
	Shadowmap,
	ScreenSpaceShadows
}
