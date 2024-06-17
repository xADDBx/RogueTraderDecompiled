using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[Serializable]
public class DecalSettings
{
	public GameObject DecalPrefab;

	public AnimationCurve AlphaOverIntensity = AnimationCurve.Linear(0f, 0f, 1f, 1f);
}
