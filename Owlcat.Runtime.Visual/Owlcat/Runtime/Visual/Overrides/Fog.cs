using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Scene/Fog")]
public class Fog : VolumeComponent, IPostProcessComponent
{
	public BoolParameter Enabled = new BoolParameter(value: false);

	public ColorParameter Color = new ColorParameter(UnityEngine.Color.gray);

	[Header("Linear Fog")]
	public MinFloatParameter StartDistance = new MinFloatParameter(0f, 0f);

	public MinFloatParameter EndDistance = new MinFloatParameter(100f, 0f);

	[Header("Exponential Fog")]
	public ClampedFloatParameter Density = new ClampedFloatParameter(0f, 0f, 1f);

	public bool IsActive()
	{
		return Enabled.value;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
