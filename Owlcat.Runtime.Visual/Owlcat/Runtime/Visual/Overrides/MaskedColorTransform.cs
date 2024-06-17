using System;
using Owlcat.Runtime.Visual.Utilities;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Masked Color Transform")]
public class MaskedColorTransform : VolumeComponent, IPostProcessComponent
{
	[Serializable]
	public class StencilRefParameter : VolumeParameter<StencilRef>
	{
		public StencilRefParameter(StencilRef value, bool overrideState = false)
			: base(value, overrideState)
		{
		}
	}

	public StencilRefParameter StencilRef = new StencilRefParameter((StencilRef)0);

	public ClampedFloatParameter Brightness = new ClampedFloatParameter(0f, 0f, 1f);

	public ClampedFloatParameter Contrast = new ClampedFloatParameter(0f, -1f, 1f);

	public bool IsActive()
	{
		return StencilRef.value != (StencilRef)0;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
