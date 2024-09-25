using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Shadows, Midtones, Highlights")]
public sealed class ShadowsMidtonesHighlights : VolumeComponent, IPostProcessComponent
{
	[Tooltip("Controls the darkest portions of the render.")]
	public Vector4Parameter shadows = new Vector4Parameter(new Vector4(1f, 1f, 1f, 0f));

	[Tooltip("Power function that controls mid-range tones.")]
	public Vector4Parameter midtones = new Vector4Parameter(new Vector4(1f, 1f, 1f, 0f));

	[Tooltip("Controls the lightest portions of the render.")]
	public Vector4Parameter highlights = new Vector4Parameter(new Vector4(1f, 1f, 1f, 0f));

	[Tooltip("Start point of the transition between shadows and midtones.")]
	public MinFloatParameter shadowsStart = new MinFloatParameter(0f, 0f);

	[Tooltip("End point of the transition between shadows and midtones.")]
	public MinFloatParameter shadowsEnd = new MinFloatParameter(0.3f, 0f);

	[Tooltip("Start point of the transition between midtones and highlights.")]
	public MinFloatParameter highlightsStart = new MinFloatParameter(0.55f, 0f);

	[Tooltip("End point of the transition between midtones and highlights.")]
	public MinFloatParameter highlightsEnd = new MinFloatParameter(1f, 0f);

	public bool IsActive()
	{
		Vector4 vector = new Vector4(1f, 1f, 1f, 0f);
		if (!(shadows != vector) && !(midtones != vector))
		{
			return highlights != vector;
		}
		return true;
	}

	public bool IsTileCompatible()
	{
		return true;
	}
}
