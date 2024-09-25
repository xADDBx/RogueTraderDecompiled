using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[Serializable]
public class WeatherDirectionalLightSettings
{
	public AnimationCurve ShadowStrengthMultiplierOverRootIntensity = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve LerpToNewColorOverRootIntensity = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public Color NewColor;
}
