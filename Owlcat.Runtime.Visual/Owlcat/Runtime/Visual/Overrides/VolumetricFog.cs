using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Scene/Volumetric Fog")]
public class VolumetricFog : VolumeComponent, IPostProcessComponent
{
	[Tooltip("Enables Volumetric Fog")]
	public BoolParameter FogEnabled = new BoolParameter(value: false);

	[Tooltip("This determines how directional the volumetric scattering is; a value of 0, means light scatters equally in all directions, while a value close to 1 causes scattering, predominantly in the direction of the light (you have to be looking at the light to see its scattering).")]
	[Indent(1)]
	public ClampedFloatParameter Anisotropy = new ClampedFloatParameter(0f, -1f, 1f);

	[Indent(1)]
	public MinFloatParameter FogDistanceAttenuation = new MinFloatParameter(5000f, 0f);

	[Tooltip("Fog color")]
	[Indent(1)]
	public ColorParameter Albedo = new ColorParameter(new Color(1f, 1f, 1f, 1f));

	public MinFloatParameter AmbientLightMultiplier = new MinFloatParameter(1f, 0f);

	public BoolParameter HeightFogEnabled = new BoolParameter(value: false);

	[Indent(1)]
	public FloatParameter BaseHeight = new FloatParameter(0f);

	[Indent(1)]
	public MinFloatParameter FogHeight = new MinFloatParameter(50f, 0f);

	public bool IsActive()
	{
		if (FogEnabled.value)
		{
			return FogDistanceAttenuation.value > 0f;
		}
		return false;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
