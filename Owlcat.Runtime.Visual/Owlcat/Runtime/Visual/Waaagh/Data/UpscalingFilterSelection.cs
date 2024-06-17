using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Data;

public enum UpscalingFilterSelection
{
	[InspectorName("Automatic")]
	[Tooltip("Unity selects a filtering option automatically based on the Render Scale value and the current screen resolution.")]
	Auto,
	[InspectorName("Bilinear")]
	Linear,
	[InspectorName("Nearest-Neighbor")]
	Point,
	[InspectorName("FidelityFX Super Resolution 1.0")]
	[Tooltip("If the target device does not support Unity shader model 4.5, Unity falls back to the Automatic option.")]
	FSR
}
