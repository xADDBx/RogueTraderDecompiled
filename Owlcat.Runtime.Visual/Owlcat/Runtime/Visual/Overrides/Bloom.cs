using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Bloom")]
public sealed class Bloom : VolumeComponent, IPostProcessComponent
{
	[Tooltip("Filters out pixels under this level of brightness. Value is in gamma-space.")]
	public MinFloatParameter threshold = new MinFloatParameter(0.9f, 0f);

	[Tooltip("Strength of the bloom filter.")]
	public MinFloatParameter intensity = new MinFloatParameter(0f, 0f);

	[Tooltip("Changes the extent of veiling effects.")]
	public ClampedFloatParameter scatter = new ClampedFloatParameter(0.7f, 0f, 1f);

	[Tooltip("Clamps pixels to control the bloom amount.")]
	public MinFloatParameter clamp = new MinFloatParameter(65472f, 0f);

	[Tooltip("Global tint of the bloom filter.")]
	public ColorParameter tint = new ColorParameter(Color.white, hdr: false, showAlpha: false, showEyeDropper: true);

	[Tooltip("Use bicubic sampling instead of bilinear sampling for the upsampling passes. This is slightly more expensive but helps getting smoother visuals.")]
	public BoolParameter highQualityFiltering = new BoolParameter(value: false);

	[Tooltip("Dirtiness texture to add smudges or dust to the bloom effect.")]
	public TextureParameter dirtTexture = new TextureParameter(null);

	[Tooltip("Amount of dirtiness.")]
	public MinFloatParameter dirtIntensity = new MinFloatParameter(0f, 0f);

	public bool IsActive()
	{
		return intensity.value > 0f;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
