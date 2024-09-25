using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[Serializable]
public class VFXLocationWeatherDataProfile
{
	public LayerMask RaycastMask = 256;

	public bool RaycastIsBlockedByOtherLayers;

	[Range(1f, 10f)]
	public int TextureDensity = 3;

	public int MaxAllowedTextureSize = 4096;

	public bool CreateCollidersForAllMeshes;
}
