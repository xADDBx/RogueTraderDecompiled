using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public enum AntialiasingMode
{
	[InspectorName("No Anti-aliasing")]
	None,
	[InspectorName("Fast Approximate Anti-aliasing (FXAA)")]
	FastApproximateAntialiasing,
	[InspectorName("Subpixel Morphological Anti-aliasing (SMAA)")]
	SubpixelMorphologicalAntiAliasing
}
