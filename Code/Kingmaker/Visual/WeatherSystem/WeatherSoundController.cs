using System;
using Kingmaker.Sound.Base;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

public class WeatherSoundController : IWeatherEntityController, IDisposable
{
	private const float SOUND_UPDATE_FREQUENCY = 1f;

	private const float WEATHER_INTENSITY_CHANGE_TOLERANCE = 0.05f;

	private const float WIND_INTENSITY_CHANGE_TOLERANCE = 0.1f;

	private float m_LastTimeSoundUpdate;

	private float m_MaxWindIntensity = 1f;

	private float m_LastWeatherIntensity;

	private float m_LastWindIntensity;

	private readonly GameObject m_Root;

	private readonly WeatherSoundType m_SoundType;

	public WeatherSoundController(Transform root, WeatherSoundType soundType)
	{
		m_Root = root.gameObject;
		m_SoundType = soundType;
		if (m_SoundType != 0)
		{
			SoundEventsManager.PostEvent(GetPlayEventName(), m_Root);
		}
		SoundEventsManager.PostEvent("WEATHER_Wind_Play", m_Root);
		m_LastTimeSoundUpdate = Time.time;
		m_MaxWindIntensity = VFXWeatherSystem.Instance.Profile.WindProfile.WindIntensityRanges.GetMaxValue();
		if (m_MaxWindIntensity <= 0.1f)
		{
			m_MaxWindIntensity = 1f;
		}
	}

	public void Update(Camera camera, float weatherIntensity, Vector2 windDirection, float windIntensity)
	{
		if (Time.time > m_LastTimeSoundUpdate + 1f || Mathf.Abs(m_LastWeatherIntensity - weatherIntensity) > 0.05f || Mathf.Abs(m_LastWindIntensity - windIntensity) > 0.1f)
		{
			m_LastWeatherIntensity = weatherIntensity;
			m_LastWindIntensity = windIntensity;
			weatherIntensity *= 10f;
			windIntensity = windIntensity / m_MaxWindIntensity * 10f;
			AkSoundEngine.SetRTPCValue("RainIntensity", weatherIntensity);
			AkSoundEngine.SetRTPCValue("WindIntensity", windIntensity);
			m_LastTimeSoundUpdate = Time.time;
		}
	}

	public void Dispose()
	{
		AkSoundEngine.SetRTPCValue("RainIntensity", 0f);
		AkSoundEngine.SetRTPCValue("WindIntensity", 0f);
		if (m_SoundType != 0)
		{
			SoundEventsManager.PostEvent(GetStopEventName(), m_Root);
		}
		SoundEventsManager.PostEvent("WEATHER_Wind_Stop", m_Root);
	}

	private string GetPlayEventName()
	{
		return m_SoundType switch
		{
			WeatherSoundType.None => string.Empty, 
			WeatherSoundType.Rain => "WEATHER_Rain_Play", 
			WeatherSoundType.Snow => "WEATHER_Snow_Play", 
			WeatherSoundType.BloodRain => "WEATHER_BloodRain_Play", 
			WeatherSoundType.Dust => "WEATHER_Dust_Play", 
			WeatherSoundType.Insects => "WEATHER_Insects_Play", 
			WeatherSoundType.InvertedRain => "WEATHER_InvertedRain_Play", 
			WeatherSoundType.Chaos => "WEATHER_Chaos_Play", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private string GetStopEventName()
	{
		return m_SoundType switch
		{
			WeatherSoundType.None => string.Empty, 
			WeatherSoundType.Rain => "WEATHER_Rain_Stop", 
			WeatherSoundType.Snow => "WEATHER_Snow_Stop", 
			WeatherSoundType.BloodRain => "WEATHER_BloodRain_Stop", 
			WeatherSoundType.Dust => "WEATHER_Dust_Stop", 
			WeatherSoundType.Insects => "WEATHER_Insects_Stop", 
			WeatherSoundType.InvertedRain => "WEATHER_InvertedRain_Stop", 
			WeatherSoundType.Chaos => "WEATHER_Chaos_Stop", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
