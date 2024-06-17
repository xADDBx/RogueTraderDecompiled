using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
public class TracingMethodParameter : VolumeParameter<TracingMethod>
{
	public TracingMethodParameter(TracingMethod value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
