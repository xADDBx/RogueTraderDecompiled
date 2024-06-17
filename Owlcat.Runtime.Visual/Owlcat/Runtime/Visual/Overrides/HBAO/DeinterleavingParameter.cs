using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides.HBAO;

[Serializable]
public sealed class DeinterleavingParameter : VolumeParameter<Deinterleaving>
{
	public DeinterleavingParameter(Deinterleaving value, bool overrideState = false)
		: base(value, overrideState)
	{
	}
}
