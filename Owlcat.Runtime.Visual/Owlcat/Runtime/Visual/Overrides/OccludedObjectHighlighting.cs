using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Renderer Features/Occluded Object Highlighting")]
public class OccludedObjectHighlighting : VolumeComponent
{
	[Tooltip("Intensity of the see through effect.")]
	public ClampedFloatParameter Intensity = new ClampedFloatParameter(1f, 0f, 1f);

	public bool IsActive()
	{
		return Intensity.value > 0f;
	}
}
