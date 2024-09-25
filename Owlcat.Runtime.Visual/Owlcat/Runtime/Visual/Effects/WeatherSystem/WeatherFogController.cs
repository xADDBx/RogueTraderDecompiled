using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public class WeatherFogController
{
	private FogSettings m_DefaultFogSettings = new FogSettings();

	private List<WeatherLayer> m_Layers;

	public WeatherFogController(List<WeatherLayer> layers)
	{
		m_DefaultFogSettings.Enabled = RenderSettings.fog;
		m_DefaultFogSettings.FogMode = RenderSettings.fogMode;
		m_DefaultFogSettings.Color = (RenderSettings.fog ? RenderSettings.fogColor : Color.gray);
		m_DefaultFogSettings.StartDistance = (RenderSettings.fog ? RenderSettings.fogStartDistance : 30f);
		m_DefaultFogSettings.EndDistance = (RenderSettings.fog ? RenderSettings.fogEndDistance : 70f);
		m_Layers = layers;
	}

	public void Update(float rootIntensity)
	{
	}
}
