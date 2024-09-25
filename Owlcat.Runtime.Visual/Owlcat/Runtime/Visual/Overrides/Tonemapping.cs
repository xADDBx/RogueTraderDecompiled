using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Tonemapping")]
public sealed class Tonemapping : VolumeComponent, IPostProcessComponent
{
	[Tooltip("Select a tonemapping algorithm to use for the color grading process.")]
	public TonemappingModeParameter mode = new TonemappingModeParameter(TonemappingMode.None);

	public ClampedFloatParameter neutralBlackIn = new ClampedFloatParameter(0.02f, -0.1f, 0.1f);

	public ClampedFloatParameter neutralWhiteIn = new ClampedFloatParameter(10f, 1f, 20f);

	public ClampedFloatParameter neutralBlackOut = new ClampedFloatParameter(0f, -0.09f, 0.1f);

	public ClampedFloatParameter neutralWhiteOut = new ClampedFloatParameter(10f, 1f, 19f);

	public ClampedFloatParameter neutralWhiteLevel = new ClampedFloatParameter(5.3f, 0.1f, 20f);

	public ClampedFloatParameter neutralWhiteClip = new ClampedFloatParameter(10f, 1f, 10f);

	public bool IsActive()
	{
		return mode.value != TonemappingMode.None;
	}

	public bool IsTileCompatible()
	{
		return true;
	}
}
