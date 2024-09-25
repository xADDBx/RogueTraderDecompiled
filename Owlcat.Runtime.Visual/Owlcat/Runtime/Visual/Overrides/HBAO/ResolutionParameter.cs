using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides.HBAO;

[Serializable]
public sealed class ResolutionParameter : VolumeParameter<Resolution>
{
	public ResolutionParameter(Resolution value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
