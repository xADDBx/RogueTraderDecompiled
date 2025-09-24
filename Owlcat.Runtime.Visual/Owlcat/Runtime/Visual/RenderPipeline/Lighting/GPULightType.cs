using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Lighting;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\RenderPipeline\\Lighting\\LightData.cs")]
public enum GPULightType
{
	Directional,
	Spot,
	Point,
	Count
}
