using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
public class ColorPrecisionParameter : VolumeParameter<ColorPrecision>
{
	public ColorPrecisionParameter(ColorPrecision value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
