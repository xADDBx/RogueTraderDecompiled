using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Panini Projection")]
public sealed class PaniniProjection : VolumeComponent, IPostProcessComponent
{
	[Tooltip("Panini projection distance.")]
	public ClampedFloatParameter distance = new ClampedFloatParameter(0f, 0f, 1f);

	[Tooltip("Panini projection crop to fit.")]
	public ClampedFloatParameter cropToFit = new ClampedFloatParameter(1f, 0f, 1f);

	public bool IsActive()
	{
		return distance.value > 0f;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
