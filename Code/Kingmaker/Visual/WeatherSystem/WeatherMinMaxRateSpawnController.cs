using System;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

public abstract class WeatherMinMaxRateSpawnController<TSettings> : IWeatherEntityController, IDisposable where TSettings : WeatherMinMaxRateSpawnSettings
{
	protected TSettings m_Settings;

	private float m_TimeSinceLastSpawn;

	public WeatherMinMaxRateSpawnController(TSettings settings, Transform root)
	{
		m_Settings = settings;
	}

	public void Update(Camera camera, float weatherIntensity, Vector2 windDirection, float windIntensity)
	{
		if (!CanSpawn())
		{
			return;
		}
		m_TimeSinceLastSpawn += Time.deltaTime;
		float num = 1f / m_Settings.EmissionRateMaxOverIntensity.Evaluate(weatherIntensity);
		float num2 = 1f / m_Settings.EmissionRateMinOverIntensity.Evaluate(weatherIntensity);
		if (m_TimeSinceLastSpawn < num)
		{
			return;
		}
		if (m_TimeSinceLastSpawn >= num2)
		{
			m_TimeSinceLastSpawn = 0f;
			Spawn(camera, weatherIntensity, windDirection, windIntensity);
			return;
		}
		float num3 = Time.deltaTime / (num2 - num);
		if (PFStatefulRandom.Weather.value <= num3)
		{
			m_TimeSinceLastSpawn = 0f;
			Spawn(camera, weatherIntensity, windDirection, windIntensity);
		}
	}

	protected abstract bool CanSpawn();

	protected abstract void Spawn(Camera camera, float weatherIntensity, Vector2 windDirection, float windIntensity);

	public virtual void Dispose()
	{
	}
}
