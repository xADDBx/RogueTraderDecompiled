using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/FilmGrain")]
public sealed class FilmGrain : VolumeComponent, IPostProcessComponent
{
	[Tooltip("The type of grain to use. You can select a preset or provide your own texture by selecting Custom.")]
	public FilmGrainLookupParameter type = new FilmGrainLookupParameter(FilmGrainLookup.Thin1);

	[Tooltip("Amount of vignetting on screen.")]
	public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

	[Tooltip("Controls the noisiness response curve based on scene luminance. Higher values mean less noise in light areas.")]
	public ClampedFloatParameter response = new ClampedFloatParameter(0.8f, 0f, 1f);

	[Tooltip("A tileable texture to use for the grain. The neutral value is 0.5 where no grain is applied.")]
	public NoInterpTextureParameter texture = new NoInterpTextureParameter(null);

	public bool IsActive()
	{
		if (intensity.value > 0f)
		{
			if (type.value == FilmGrainLookup.Custom)
			{
				return texture.value != null;
			}
			return true;
		}
		return false;
	}

	public bool IsTileCompatible()
	{
		return true;
	}
}
