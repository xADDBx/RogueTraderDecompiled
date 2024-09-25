using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

[CreateAssetMenu(menuName = "VFX Weather System/Camera Shake Effect")]
public class WeatherCameraShakeSettings : WeatherCustomEntitySettings
{
	public CameraShakeFx CameraShakeFxPrefab;

	public AnimationCurve AmplitudeOverIntensity = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve FreqOverIntensity = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public override IWeatherEntityController GetController(Transform root)
	{
		return new WeatherCameraShakeController(this, root);
	}
}
