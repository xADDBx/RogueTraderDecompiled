using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
public sealed class MotionBlurQualityParameter : VolumeParameter<MotionBlurQuality>
{
	public MotionBlurQualityParameter(MotionBlurQuality value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
