using System;
using Owlcat.Runtime.Visual.Waaagh.Shadows;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting;

[Serializable]
public class VolumetricLightingSettings
{
	[Header("Quality")]
	public VolumetricLightingSlices Slices = VolumetricLightingSlices.x64;

	public float FarClip = 50f;

	public bool TemporalAccumulation;

	[Range(0f, 1f)]
	public float TemporalFeedback = 0.05f;

	public bool LocalVolumesEnabled;

	public bool TricubicFilteringDeferred;

	public bool TricubicFilteringForward;

	public bool UseDownsampledShadowmap;

	public Owlcat.Runtime.Visual.Waaagh.Shadows.ShadowResolution DownsampledShadowmapSize = Owlcat.Runtime.Visual.Waaagh.Shadows.ShadowResolution._256;

	public bool DebugLocalVolumetricFog;

	[Header("Lights")]
	public VolumetricLightShadows LightShadows;
}
