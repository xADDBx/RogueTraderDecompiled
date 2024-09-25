using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Slope, Power, Offset")]
public class SlopePowerOffset : VolumeComponent, IPostProcessComponent
{
	[Tooltip("Controls the darkest portions of the render.")]
	public Vector4Parameter slope = new Vector4Parameter(new Vector4(1f, 1f, 1f, 0f));

	[Tooltip("Power function that controls mid-range tones.")]
	public Vector4Parameter power = new Vector4Parameter(new Vector4(1f, 1f, 1f, 0f));

	[Tooltip("Controls the lightest portions of the render.")]
	public Vector4Parameter offset = new Vector4Parameter(new Vector4(1f, 1f, 1f, 0f));

	public bool IsActive()
	{
		Vector4 vector = new Vector4(1f, 1f, 1f, 0f);
		if (!(slope != vector) && !(power != vector))
		{
			return offset != vector;
		}
		return true;
	}

	public bool IsTileCompatible()
	{
		return true;
	}
}
