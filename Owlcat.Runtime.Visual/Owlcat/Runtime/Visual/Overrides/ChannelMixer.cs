using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Channel Mixer")]
public sealed class ChannelMixer : VolumeComponent, IPostProcessComponent
{
	[Tooltip("Modify influence of the red channel in the overall mix.")]
	public ClampedFloatParameter redOutRedIn = new ClampedFloatParameter(100f, -200f, 200f);

	[Tooltip("Modify influence of the green channel in the overall mix.")]
	public ClampedFloatParameter redOutGreenIn = new ClampedFloatParameter(0f, -200f, 200f);

	[Tooltip("Modify influence of the blue channel in the overall mix.")]
	public ClampedFloatParameter redOutBlueIn = new ClampedFloatParameter(0f, -200f, 200f);

	[Tooltip("Modify influence of the red channel in the overall mix.")]
	public ClampedFloatParameter greenOutRedIn = new ClampedFloatParameter(0f, -200f, 200f);

	[Tooltip("Modify influence of the green channel in the overall mix.")]
	public ClampedFloatParameter greenOutGreenIn = new ClampedFloatParameter(100f, -200f, 200f);

	[Tooltip("Modify influence of the blue channel in the overall mix.")]
	public ClampedFloatParameter greenOutBlueIn = new ClampedFloatParameter(0f, -200f, 200f);

	[Tooltip("Modify influence of the red channel in the overall mix.")]
	public ClampedFloatParameter blueOutRedIn = new ClampedFloatParameter(0f, -200f, 200f);

	[Tooltip("Modify influence of the green channel in the overall mix.")]
	public ClampedFloatParameter blueOutGreenIn = new ClampedFloatParameter(0f, -200f, 200f);

	[Tooltip("Modify influence of the blue channel in the overall mix.")]
	public ClampedFloatParameter blueOutBlueIn = new ClampedFloatParameter(100f, -200f, 200f);

	public bool IsActive()
	{
		if (redOutRedIn.value == 100f && redOutGreenIn.value == 0f && redOutBlueIn.value == 0f && greenOutRedIn.value == 0f && greenOutGreenIn.value == 100f && greenOutBlueIn.value == 0f && blueOutRedIn.value == 0f && blueOutGreenIn.value == 0f)
		{
			return blueOutBlueIn.value != 100f;
		}
		return true;
	}

	public bool IsTileCompatible()
	{
		return true;
	}
}
