using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/TaaSharpness")]
public class TaaSharpness : VolumeComponent, IPostProcessComponent
{
	public ClampedFloatParameter Intensity = new ClampedFloatParameter(0f, 0f, 1f);

	public ClampedFloatParameter Sharpness = new ClampedFloatParameter(0.92f, 0f, 1f);

	public float GetSharpness()
	{
		return Intensity.value * Sharpness.value;
	}

	public bool IsActive()
	{
		return GetSharpness() > 0f;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
