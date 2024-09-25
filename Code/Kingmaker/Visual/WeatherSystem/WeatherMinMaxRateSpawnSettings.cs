using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

public abstract class WeatherMinMaxRateSpawnSettings : WeatherCustomEntitySettings
{
	public AnimationCurve EmissionRateMinOverIntensity = AnimationCurve.Linear(0f, 0f, 1f, 0.1f);

	public AnimationCurve EmissionRateMaxOverIntensity = AnimationCurve.Linear(0f, 0f, 1f, 0.1f);
}
