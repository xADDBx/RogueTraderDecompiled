using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[Serializable]
public class FogSettings
{
	public bool Enabled = true;

	public AnimationCurve FogIntensityOverRootIntensity = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[NonSerialized]
	public FogMode FogMode = FogMode.Linear;

	public Color Color = new Color(0.5f, 0.5f, 0.5f, 1f);

	public float StartDistance;

	public float EndDistance = 300f;
}
