using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public class WeatherDirectionalLightController : IWeatherEntityController, IDisposable
{
	private readonly WeatherDirectionalLightSettings m_Settings;

	private readonly Light m_Light;

	private readonly float m_OriginalShadowStrength;

	private readonly Color m_OriginalColor;

	public WeatherDirectionalLightController(WeatherDirectionalLightSettings settings, Light light)
	{
		m_Settings = settings;
		m_Light = light;
		m_OriginalShadowStrength = light.shadowStrength;
		m_OriginalColor = light.color;
	}

	public void Update(Camera camera, float weatherIntensity, Vector2 windDirection, float windIntensity)
	{
		if ((bool)m_Light)
		{
			m_Light.shadowStrength = m_OriginalShadowStrength * m_Settings.ShadowStrengthMultiplierOverRootIntensity.Evaluate(weatherIntensity);
			m_Light.color = Color.Lerp(m_OriginalColor, m_Settings.NewColor, m_Settings.LerpToNewColorOverRootIntensity.Evaluate(weatherIntensity));
		}
	}

	public void Dispose()
	{
		if (m_Light != null)
		{
			m_Light.shadowStrength = m_OriginalShadowStrength;
			m_Light.color = m_OriginalColor;
		}
	}
}
