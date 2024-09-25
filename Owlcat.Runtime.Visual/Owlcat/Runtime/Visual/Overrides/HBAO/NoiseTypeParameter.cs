using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides.HBAO;

[Serializable]
public sealed class NoiseTypeParameter : VolumeParameter<NoiseType>
{
	public NoiseTypeParameter(NoiseType value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
