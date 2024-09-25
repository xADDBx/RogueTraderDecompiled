using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Bloom Enhanced")]
public sealed class BloomEnhanced : VolumeComponent, IPostProcessComponent
{
	[Tooltip("Blend factor of the result image.")]
	public MinFloatParameter intensity = new MinFloatParameter(0f, 0f);

	[Tooltip("Filters out pixels under this level of brightness.")]
	public MinFloatParameter threshold = new MinFloatParameter(1.1f, 0f);

	[Tooltip("Makes transition between under/over-threshold gradual (0 = hard threshold, 1 = soft threshold).")]
	public ClampedFloatParameter softKnee = new ClampedFloatParameter(0.5f, 0f, 1f);

	[Tooltip("Changes extent of veiling effects in a screen resolution-independent fashion.")]
	public ClampedFloatParameter radius = new ClampedFloatParameter(4f, 1f, 7f);

	[Tooltip("Reduces flashing noise with an additional filter.")]
	public BoolParameter antiFlicker = new BoolParameter(value: false);

	[Tooltip("Global tint of the bloom filter.")]
	public ColorParameter tint = new ColorParameter(Color.white, hdr: false, showAlpha: false, showEyeDropper: true);

	[Tooltip("Clamps pixels to control the bloom amount.")]
	public MinFloatParameter clamp = new MinFloatParameter(65472f, 0f);

	[Tooltip("Dirtiness texture to add smudges or dust to the bloom effect.")]
	public TextureParameter dirtTexture = new TextureParameter(null);

	[Tooltip("Amount of dirtiness.")]
	public MinFloatParameter dirtIntensity = new MinFloatParameter(0f, 0f);

	public float thresholdLinear
	{
		get
		{
			return Mathf.GammaToLinearSpace(threshold.value);
		}
		set
		{
			threshold.value = Mathf.LinearToGammaSpace(value);
		}
	}

	public bool IsActive()
	{
		return intensity.value > 0f;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
