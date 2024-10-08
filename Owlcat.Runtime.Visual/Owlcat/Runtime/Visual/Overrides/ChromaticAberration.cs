using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Chromatic Aberration")]
public sealed class ChromaticAberration : VolumeComponent, IPostProcessComponent
{
	[Tooltip("Amount of tangential distortion.")]
	public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

	public bool IsActive()
	{
		return intensity.value > 0f;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
