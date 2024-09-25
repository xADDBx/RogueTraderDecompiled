using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides.HBAO;

[Serializable]
[VolumeComponentMenu("Post-processing/HBAO")]
public class Hbao : VolumeComponent, IPostProcessComponent
{
	[Tooltip("The quality of the AO.")]
	public QualityParameter Quality = new QualityParameter(Owlcat.Runtime.Visual.Overrides.HBAO.Quality.Medium);

	[Tooltip("The deinterleaving factor.")]
	public DeinterleavingParameter Deinterleaving = new DeinterleavingParameter(Owlcat.Runtime.Visual.Overrides.HBAO.Deinterleaving.Disabled);

	[Tooltip("The resolution at which the AO is calculated.")]
	public ResolutionParameter Resolution = new ResolutionParameter(Owlcat.Runtime.Visual.Overrides.HBAO.Resolution.Full);

	[Tooltip("The type of noise to use.")]
	public NoiseTypeParameter NoiseType = new NoiseTypeParameter(Owlcat.Runtime.Visual.Overrides.HBAO.NoiseType.Dither);

	[Tooltip("AO radius: this is the distance outside which occluders are ignored.")]
	public ClampedFloatParameter Radius = new ClampedFloatParameter(0.8f, 0f, 2f);

	[Tooltip("Maximum radius in pixels: this prevents the radius to grow too much with close-up object and impact on performances.")]
	public ClampedFloatParameter MaxRadiusPixels = new ClampedFloatParameter(128f, 64f, 512f);

	[Tooltip("For low-tessellated geometry, occlusion variations tend to appear at creases and ridges, which betray the underlying tessellation. To remove these artifacts, we use an angle bias parameter which restricts the hemisphere.")]
	public ClampedFloatParameter Bias = new ClampedFloatParameter(0.05f, 0f, 0.5f);

	[Tooltip("This value allows to scale up the ambient occlusion values.")]
	public ClampedFloatParameter Intensity = new ClampedFloatParameter(0f, 0f, 10f);

	[Tooltip("Enable/disable MultiBounce approximation.")]
	public BoolParameter UseMultiBounce = new BoolParameter(value: false);

	[Tooltip("MultiBounce approximation influence.")]
	public ClampedFloatParameter MultiBounceInfluence = new ClampedFloatParameter(1f, 0f, 1f);

	[Tooltip("The max distance to display AO.")]
	public FloatParameter MaxDistance = new FloatParameter(150f);

	[Tooltip("The distance before max distance at which AO start to decrease.")]
	public FloatParameter DistanceFalloff = new FloatParameter(50f);

	[Tooltip("This setting allow you to set the base color if the AO, the alpha channel value is unused.")]
	public ColorParameter BaseColor = new ColorParameter(Color.black);

	public BoolParameter ColorBleedingEnabled = new BoolParameter(value: false);

	[Tooltip("This value allows to control the saturation of the color bleeding.")]
	public ClampedFloatParameter Saturation = new ClampedFloatParameter(1f, 0f, 4f);

	[Tooltip("This value allows to scale the contribution of the color bleeding samples.")]
	public ClampedFloatParameter AlbedoMultiplier = new ClampedFloatParameter(4f, 0f, 32f);

	[Tooltip("Use masking on emissive pixels")]
	public ClampedFloatParameter BrightnessMask = new ClampedFloatParameter(1f, 0f, 1f);

	[Tooltip("Brightness level where masking starts/ends")]
	public Vector2Parameter BrightnessMaskRange = new Vector2Parameter(new Vector2(0.8f, 1.2f));

	[Tooltip("The type of blur to use.")]
	public BlurParameter BlurAmount = new BlurParameter(Blur.Medium);

	[Tooltip("This parameter controls the depth-dependent weight of the bilateral filter, to avoid bleeding across edges. A zero sharpness is a pure Gaussian blur. Increasing the blur sharpness removes bleeding by using lower weights for samples with large depth delta from the current pixel.")]
	public ClampedFloatParameter Sharpness = new ClampedFloatParameter(8f, 0f, 16f);

	[Tooltip("Is the blur downsampled.")]
	public BoolParameter Downsample = new BoolParameter(value: false);

	public bool IsActive()
	{
		return Intensity.value > 0f;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
