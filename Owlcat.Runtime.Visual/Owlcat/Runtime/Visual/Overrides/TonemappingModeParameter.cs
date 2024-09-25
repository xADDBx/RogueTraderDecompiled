using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
public sealed class TonemappingModeParameter : VolumeParameter<TonemappingMode>
{
	public TonemappingModeParameter(TonemappingMode value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
