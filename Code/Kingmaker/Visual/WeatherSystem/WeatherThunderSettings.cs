using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

[CreateAssetMenu(menuName = "VFX Weather System/Thunder Distant Effect")]
public class WeatherThunderSettings : WeatherMinMaxRateSpawnSettings
{
	public AnimationCurve AlphaOverIntensity = AnimationCurve.Linear(0f, 0f, 1f, 0.1f);

	public ParticleSystem ThunderPrefab;

	public override IWeatherEntityController GetController(Transform root)
	{
		return new WeatherThunderController(this, root);
	}
}
