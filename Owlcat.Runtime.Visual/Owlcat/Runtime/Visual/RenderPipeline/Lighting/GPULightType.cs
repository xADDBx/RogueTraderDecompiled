using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Lighting;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@1.1.232\\Runtime\\RenderPipeline\\Lighting\\LightData.cs")]
public enum GPULightType
{
	Directional,
	Spot,
	Point,
	Count
}
