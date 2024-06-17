using System;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Controllers;

public class InclemencyController
{
	private IInclemencyDependentSystem m_DependentSystem;

	private Player.WeatherData m_WeatherData;

	private SeasonalData m_SeasonData;

	private Action<InclemencyType> m_OnActualInclemencyChanged;

	private float m_InclemencyLerp;

	private InclemencyType m_InclemencyPrevious;

	private InclemencyType m_InclemencyNext;

	private bool m_OverrideInclemency;

	private float? m_OverrideChangeSpeed;

	private bool m_OnStopChange;

	public InclemencyType ActualInclemency
	{
		get
		{
			if (!(m_InclemencyLerp > 0.5f))
			{
				return m_InclemencyPrevious;
			}
			return m_InclemencyNext;
		}
	}

	public InclemencyType TargetInclemency => m_InclemencyNext;

	public InclemencyController(IInclemencyDependentSystem dependentSystem, Player.WeatherData weatherData, SeasonalData seasonData)
	{
		m_DependentSystem = dependentSystem;
		m_WeatherData = weatherData;
		m_SeasonData = seasonData;
	}

	public InclemencyController(IInclemencyDependentSystem dependentSystem, Player.WeatherData weatherData, SeasonalData seasonData, Action<InclemencyType> onActualInclemencyChanged)
		: this(dependentSystem, weatherData, seasonData)
	{
		m_OnActualInclemencyChanged = onActualInclemencyChanged;
	}

	public void Tick()
	{
		if (CanTick())
		{
			if (!Game.Instance.CurrentlyLoadedAreaPart.VailAffectsTheWeather && !VFXWeatherSystem.Instance.IsProfileOverriden && (m_WeatherData.CurrentWeather != m_InclemencyNext || Game.Instance.Player.GameTime > m_WeatherData.NextWeatherChange))
			{
				InclemencyType nextInclemency = GetNextInclemency();
				SetNewInclemency(nextInclemency);
			}
			else if (m_InclemencyLerp < 1f)
			{
				ShiftInclemencyFromOldToNew();
			}
		}
	}

	private bool CanTick()
	{
		if (!m_OverrideInclemency && m_DependentSystem != null)
		{
			return m_SeasonData != null;
		}
		return false;
	}

	private InclemencyType Limit(InclemencyType inclemency)
	{
		BlueprintAreaPart currentlyLoadedAreaPart = Game.Instance.CurrentlyLoadedAreaPart;
		InclemencyType inclemencyType = (InclemencyType)Mathf.Min((int)currentlyLoadedAreaPart.WeatherInclemencyMin, (int)currentlyLoadedAreaPart.WeatherInclemencyMax);
		InclemencyType inclemencyType2 = (InclemencyType)Mathf.Max((int)currentlyLoadedAreaPart.WeatherInclemencyMin, (int)currentlyLoadedAreaPart.WeatherInclemencyMax);
		if (inclemency >= inclemencyType)
		{
			if (inclemency <= inclemencyType2)
			{
				return inclemency;
			}
			return inclemencyType2;
		}
		return inclemencyType;
	}

	private InclemencyType GetNextInclemency()
	{
		BlueprintAreaPart currentlyLoadedAreaPart = Game.Instance.CurrentlyLoadedAreaPart;
		int num = Mathf.Min((int)currentlyLoadedAreaPart.WeatherInclemencyMin, (int)currentlyLoadedAreaPart.WeatherInclemencyMax);
		int num2 = Mathf.Max((int)currentlyLoadedAreaPart.WeatherInclemencyMin, (int)currentlyLoadedAreaPart.WeatherInclemencyMax);
		if (num == num2)
		{
			return (InclemencyType)num;
		}
		float sample = PFStatefulRandom.Controllers.Inclemency.Range(0f, m_SeasonData.InclemencyWeights.Values.Sum());
		return (InclemencyType)(num + m_SeasonData.InclemencyWeights.Values.TakeWhile((float w) => (sample -= w) > 0f).Count());
	}

	public void StartOverrideInclemency(InclemencyType inclemency)
	{
		m_OverrideInclemency = true;
		m_InclemencyPrevious = inclemency;
		m_InclemencyNext = inclemency;
		m_InclemencyLerp = 1f;
		m_DependentSystem.SetInclemency(inclemency);
		m_DependentSystem.SetInclemencyChangePercentage(m_InclemencyLerp);
		m_OnActualInclemencyChanged?.Invoke(ActualInclemency);
	}

	public void StopOverrideInclemency()
	{
		m_OverrideInclemency = false;
	}

	public void SetNewInclemency(bool instantly)
	{
		SetNewInclemency(GetNextInclemency(), instantly);
	}

	public void SetNewInclemency(InclemencyType inclemency)
	{
		SetNewInclemency(inclemency, instantly: false);
	}

	public void SetNewInclemency(InclemencyType inclemency, bool instantly, bool onStop)
	{
		m_OnStopChange = onStop;
		SetNewInclemency(inclemency, instantly);
	}

	public void SetNewInclemency(InclemencyType inclemency, bool instantly, float? changeSpeed = null)
	{
		float num = 0f;
		if (!instantly)
		{
			float num2 = m_SeasonData.InclemencyChangeSpeed[m_WeatherData.CurrentWeather];
			float num3 = m_SeasonData.InclemencyChangeSpeed[inclemency];
			num = (num2 + num3) * 0.5f;
			m_OverrideChangeSpeed = changeSpeed;
		}
		InclemencyType inclemencyType = Limit(inclemency);
		m_InclemencyPrevious = Limit(m_WeatherData.CurrentWeather);
		m_InclemencyNext = inclemencyType;
		m_InclemencyLerp = (instantly ? 1f : 0f);
		if (VFXWeatherSystem.Instance.IsProfileOverriden)
		{
			m_WeatherData.CurrentWeatherEffect = inclemency;
		}
		else
		{
			m_WeatherData.CurrentWeather = inclemency;
		}
		if (m_OnStopChange)
		{
			inclemencyType = InclemencyType.Clear;
		}
		float num4 = SampleWeather(m_SeasonData.InclemencyCooldownInMinutes, inclemencyType);
		m_WeatherData.NextWeatherChange = Game.Instance.Player.GameTime + TimeSpan.FromMinutes(num4) + TimeSpan.FromSeconds(num);
		m_DependentSystem.SetInclemency(inclemencyType);
		m_DependentSystem.SetInclemencyChangePercentage(m_InclemencyLerp);
		m_OnStopChange = false;
	}

	private static float SampleWeather(WeatherMinMaxArray weatherMinMaxArray, InclemencyType inclemency)
	{
		if (weatherMinMaxArray.MinValues.Length != 0 && weatherMinMaxArray.MaxValues.Length != 0)
		{
			return PFStatefulRandom.Weather.Range(weatherMinMaxArray.MinValues[(int)inclemency], weatherMinMaxArray.MaxValues[(int)inclemency]);
		}
		return 0f;
	}

	private void ShiftInclemencyFromOldToNew()
	{
		InclemencyType actualInclemency = ActualInclemency;
		float num = m_OverrideChangeSpeed ?? m_SeasonData.InclemencyChangeSpeed[actualInclemency];
		m_InclemencyLerp = Mathf.MoveTowards(m_InclemencyLerp, 1f, Game.Instance.TimeController.DeltaTime * num);
		m_DependentSystem.SetInclemencyChangePercentage(m_InclemencyLerp);
		if (actualInclemency != ActualInclemency)
		{
			m_OnActualInclemencyChanged?.Invoke(ActualInclemency);
		}
	}
}
