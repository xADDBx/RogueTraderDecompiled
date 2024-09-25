using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.Data;

public enum VolumeFrameworkUpdateMode
{
	[InspectorName("Every Frame")]
	EveryFrame,
	[InspectorName("Via Scripting")]
	ViaScripting,
	[InspectorName("Use Pipeline Settings")]
	UsePipelineSettings
}
