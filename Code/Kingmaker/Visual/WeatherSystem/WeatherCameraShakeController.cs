using System;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

public class WeatherCameraShakeController : IWeatherEntityController, IDisposable
{
	private WeatherCameraShakeSettings m_Settings;

	private CameraShakeFx m_ShakeFx;

	public WeatherCameraShakeController(WeatherCameraShakeSettings settings, Transform root)
	{
		m_Settings = settings;
		m_ShakeFx = UnityEngine.Object.Instantiate(m_Settings.CameraShakeFxPrefab);
		m_ShakeFx.transform.parent = root;
	}

	public void Update(Camera camera, float weatherIntensity, Vector2 windDirection, float windIntensity)
	{
		m_ShakeFx.AmplitudeMultiplier = m_Settings.AmplitudeOverIntensity.Evaluate(weatherIntensity);
		m_ShakeFx.FreqMultiplier = m_Settings.FreqOverIntensity.Evaluate(weatherIntensity);
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(m_ShakeFx.gameObject);
	}
}
