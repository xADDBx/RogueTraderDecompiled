using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Lift, Gamma, Gain")]
public sealed class LiftGammaGain : VolumeComponent, IPostProcessComponent
{
	[Tooltip("Controls the darkest portions of the render.")]
	public Vector4Parameter lift = new Vector4Parameter(new Vector4(1f, 1f, 1f, 0f));

	[Tooltip("Power function that controls mid-range tones.")]
	public Vector4Parameter gamma = new Vector4Parameter(new Vector4(1f, 1f, 1f, 0f));

	[Tooltip("Controls the lightest portions of the render.")]
	public Vector4Parameter gain = new Vector4Parameter(new Vector4(1f, 1f, 1f, 0f));

	public bool IsActive()
	{
		Vector4 vector = new Vector4(1f, 1f, 1f, 0f);
		if (!(lift != vector) && !(gamma != vector))
		{
			return gain != vector;
		}
		return true;
	}

	public bool IsTileCompatible()
	{
		return true;
	}
}
