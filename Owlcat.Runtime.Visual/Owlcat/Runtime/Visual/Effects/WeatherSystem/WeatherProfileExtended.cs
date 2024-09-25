using System;
using System.Linq;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[Serializable]
public class WeatherProfileExtended : IWeatherProfile
{
	[SerializeField]
	private WeatherProfile m_WeatherProfile;

	[SerializeField]
	private WeatherLayer[] m_Layers;

	public SeasonalData SeasonalData => m_WeatherProfile?.SeasonalData;

	public WeatherLayer[] Layers => m_Layers;

	public WeatherCustomEntitySettings[] CustomEffects => m_WeatherProfile?.CustomEffects;

	public WindProfile WindProfile => m_WeatherProfile?.WindProfile;

	public WeatherMinMaxArray InclemencyIntensityRanges => m_WeatherProfile?.InclemencyIntensityRanges;

	public WeatherDirectionalLightSettings DirectionalLightSettings => m_WeatherProfile?.DirectionalLightSettings;

	public VFXLocationWeatherDataProfile BakeProfile => m_WeatherProfile?.BakeProfile;

	public string Name
	{
		get
		{
			if (!(m_WeatherProfile != null))
			{
				return "";
			}
			return m_WeatherProfile.name;
		}
	}

	public string LegacyName
	{
		get
		{
			if (!(m_WeatherProfile != null))
			{
				return "";
			}
			return m_WeatherProfile.LegacyName;
		}
	}

	public bool HasData()
	{
		if (!m_WeatherProfile)
		{
			return m_Layers?.Any() ?? false;
		}
		return true;
	}

	public bool IsEmpty()
	{
		return m_WeatherProfile == null;
	}
}
