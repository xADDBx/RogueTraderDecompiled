using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Radial Blur")]
public class RadialBlur : VolumeComponent, IPostProcessComponent
{
	public MinFloatParameter Width = new MinFloatParameter(0f, 0f);

	public MinFloatParameter Strength = new MinFloatParameter(0f, 0f);

	public Vector2Parameter Center = new Vector2Parameter(new Vector2(0.5f, 0.5f));

	public bool IsActive()
	{
		if (Strength.value > 0f)
		{
			return Width.value > 0f;
		}
		return false;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
