using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Designers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Sound.Base;
using Kingmaker.UnitLogic.FactLogic;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Controllers;

public class WeatherController : IControllerTick, IController, IControllerStart, IControllerStop, IAreaPartHandler, ISubscriber, IGameTimeAdvancedHandler, IPsychicPhenomenaUIHandler, IPartyCombatHandler, IGlobalRulebookHandler<RuleCachedPerceptionCheck>, IRulebookHandler<RuleCachedPerceptionCheck>, IGlobalRulebookSubscriber
{
	private InclemencyController m_WeatherInclemencyController;

	private InclemencyController m_WindInclemencyController;

	private float m_LastWeatherIntencity;

	private bool m_IsSoundEventStarted;

	private bool m_IsWeatherWarp;

	public static WeatherController Instance { get; private set; }

	public InclemencyType ActualInclemency => m_WeatherInclemencyController?.ActualInclemency ?? InclemencyType.Clear;

	public void OnStart()
	{
		if (Instance != null)
		{
			return;
		}
		Instance = this;
		if (!(VFXWeatherSystem.Instance == null))
		{
			if ((Game.Instance.CurrentlyLoadedAreaPart ?? Game.Instance.CurrentlyLoadedArea) == null)
			{
				PFLog.TechArt.Error("Area is null! Can't load bounds and weather");
				return;
			}
			SetArea();
			SetAreaBounds();
		}
	}

	public void OnStop()
	{
		if (Instance == this)
		{
			Instance = null;
			try
			{
				VFXWeatherSystem.Instance.OnDebugSetInclemency -= m_WeatherInclemencyController.SetNewInclemency;
			}
			catch
			{
			}
			if (!(VFXWeatherSystem.Instance == null))
			{
				m_WeatherInclemencyController.SetNewInclemency(InclemencyType.Clear, instantly: true, onStop: true);
				m_WindInclemencyController.SetNewInclemency(InclemencyType.Clear, instantly: true, onStop: true);
				m_IsSoundEventStarted = false;
				SoundEventsManager.PostEvent(BlueprintWarhammerRoot.Instance.WarpWeatherRoot.WeatherSoundEventStop, VFXWeatherSystem.Instance.gameObject);
				AkSoundEngine.SetRTPCValue("WeatherIntensity", 0f);
				m_WeatherInclemencyController = null;
				m_WindInclemencyController = null;
			}
		}
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		m_WeatherInclemencyController?.Tick();
		m_WindInclemencyController?.Tick();
		if (m_IsWeatherWarp)
		{
			UpdateWarpWeatherAudio();
		}
		if (m_IsWeatherWarp && m_LastWeatherIntencity != VFXWeatherSystem.Instance.CurrentWeatherIntensity)
		{
			m_LastWeatherIntencity = VFXWeatherSystem.Instance.CurrentWeatherIntensity;
			AkSoundEngine.SetRTPCValue("WeatherIntensity", m_LastWeatherIntencity);
		}
	}

	private void UpdateWarpWeatherAudio()
	{
		if (VFXWeatherSystem.Instance.CurrentWeatherIntensity > BlueprintWarhammerRoot.Instance.WarpWeatherRoot.WeatherThreshold)
		{
			if (!m_IsSoundEventStarted)
			{
				m_IsSoundEventStarted = true;
				SoundEventsManager.PostEvent(BlueprintWarhammerRoot.Instance.WarpWeatherRoot.WeatherSoundEventStart, VFXWeatherSystem.Instance.gameObject);
			}
		}
		else if (m_IsSoundEventStarted)
		{
			m_IsSoundEventStarted = false;
			SoundEventsManager.PostEvent(BlueprintWarhammerRoot.Instance.WarpWeatherRoot.WeatherSoundEventStop, VFXWeatherSystem.Instance.gameObject);
		}
	}

	private void OnActualInclemencyChanged(InclemencyType inclemency)
	{
		if (VFXWeatherSystem.Instance.IsProfileOverriden)
		{
			Game.Instance.Player.Weather.CurrentWeatherEffect = inclemency;
		}
		else
		{
			Game.Instance.Player.Weather.CurrentWeather = inclemency;
		}
		EventBus.RaiseEvent(delegate(IWeatherChangeHandler h)
		{
			h.OnWeatherChange();
		});
	}

	private void OnActualWindChanged(InclemencyType inclemency)
	{
		Game.Instance.Player.Wind.CurrentWeather = inclemency;
		EventBus.RaiseEvent(delegate(IWeatherChangeHandler h)
		{
			h.OnWeatherChange();
		});
	}

	public void HandleGameTimeAdvanced(TimeSpan deltaTime)
	{
		BlueprintAreaPart blueprintAreaPart = Game.Instance.CurrentlyLoadedAreaPart ?? Game.Instance.CurrentlyLoadedArea;
		if (Game.Instance.Player.GameTime > Game.Instance.Player.Weather.NextWeatherChange && !blueprintAreaPart.VailAffectsTheWeather)
		{
			m_WeatherInclemencyController.SetNewInclemency(instantly: true);
			m_WindInclemencyController.SetNewInclemency(instantly: true);
		}
	}

	public void OnAreaPartChanged(BlueprintAreaPart previous)
	{
		SetArea();
		SetAreaBounds();
	}

	private void SetArea()
	{
		try
		{
			VFXWeatherSystem.Instance.OnDebugSetInclemency -= m_WeatherInclemencyController.SetNewInclemency;
		}
		catch
		{
		}
		IInclemencyDependentSystem systemForWeatherController = VFXWeatherSystem.Instance.SystemForWeatherController;
		IInclemencyDependentSystem systemForWindController = VFXWeatherSystem.Instance.SystemForWindController;
		BlueprintAreaPart blueprintAreaPart = Game.Instance.CurrentlyLoadedAreaPart ?? Game.Instance.CurrentlyLoadedArea;
		if (blueprintAreaPart == null)
		{
			PFLog.TechArt.Error("Area is null! Can't load weather");
			return;
		}
		WeatherProfileExtended weatherProfile = blueprintAreaPart.WeatherProfile;
		if (weatherProfile == null || weatherProfile.IsEmpty())
		{
			if (BlueprintWarhammerRoot.Instance.WarpWeatherRoot?.WeatherProfile == null)
			{
				return;
			}
			weatherProfile = BlueprintWarhammerRoot.Instance.WarpWeatherRoot.WeatherProfile;
		}
		m_IsWeatherWarp = weatherProfile == BlueprintWarhammerRoot.Instance.WarpWeatherRoot?.WeatherProfile;
		m_WeatherInclemencyController = new InclemencyController(systemForWeatherController, Game.Instance.Player.Weather, weatherProfile.SeasonalData, OnActualInclemencyChanged);
		m_WindInclemencyController = new InclemencyController(systemForWindController, Game.Instance.Player.Wind, weatherProfile.WindProfile.SeasonalData, OnActualWindChanged);
		InclemencyType inclemency = Game.Instance.Player.Weather.CurrentWeather;
		InclemencyType inclemency2 = Game.Instance.Player.Wind.CurrentWeather;
		if (Game.Instance.Player.Weather.VisualStateEffectType.HasValue && Game.Instance.Player.Weather.CurrentWeatherEffect.HasValue)
		{
			SetWeatherVisualEffect(Game.Instance.Player.Weather.CurrentWeatherEffect.Value, Game.Instance.Player.Weather.VisualStateEffectType.Value);
			inclemency = Game.Instance.Player.Weather.CurrentWeatherEffect.Value;
		}
		if (blueprintAreaPart.VailAffectsTheWeather && BlueprintWarhammerRoot.Instance.WarpWeatherRoot != null && !VFXWeatherSystem.Instance.IsProfileOverriden)
		{
			inclemency = (inclemency2 = BlueprintWarhammerRoot.Instance.WarpWeatherRoot.GetInclemencyTypeByVail(blueprintAreaPart.StartVailValueForLocation));
		}
		m_WeatherInclemencyController.SetNewInclemency(inclemency, instantly: true);
		m_WindInclemencyController.SetNewInclemency(inclemency2, instantly: true);
		VFXWeatherSystem.Instance.OnDebugSetInclemency += m_WeatherInclemencyController.SetNewInclemency;
	}

	private void SetAreaBounds()
	{
		BlueprintAreaPart blueprintAreaPart = Game.Instance.CurrentlyLoadedAreaPart ?? Game.Instance.CurrentlyLoadedArea;
		if (blueprintAreaPart == null)
		{
			PFLog.TechArt.Error("Area is null! Can't load bounds");
			return;
		}
		Bounds bakedGroundBounds = blueprintAreaPart.Bounds.BakedGroundBounds;
		VFXWeatherSystem.Instance.SetAreaBounds(bakedGroundBounds);
	}

	public void StartOverrideInclemency(InclemencyType inclemency)
	{
		m_WeatherInclemencyController?.StartOverrideInclemency(inclemency);
	}

	public void StopOverrideInclemency()
	{
		m_WeatherInclemencyController?.StopOverrideInclemency();
	}

	public void OnEventAboutToTrigger(RuleCachedPerceptionCheck evt)
	{
	}

	public void OnEventDidTrigger(RuleCachedPerceptionCheck evt)
	{
	}

	public void HandleVeilThicknessValueChanged(int delta, int value)
	{
		if ((Game.Instance.CurrentlyLoadedAreaPart ?? Game.Instance.CurrentlyLoadedArea).VailAffectsTheWeather && BlueprintWarhammerRoot.Instance.WarpWeatherRoot != null)
		{
			InclemencyType inclemencyTypeByVail = BlueprintWarhammerRoot.Instance.WarpWeatherRoot.GetInclemencyTypeByVail(value);
			if (VFXWeatherSystem.Instance.IsProfileOverriden)
			{
				Game.Instance.Player.Weather.CurrentWeather = inclemencyTypeByVail;
			}
			else if (m_WeatherInclemencyController.TargetInclemency != inclemencyTypeByVail)
			{
				m_WeatherInclemencyController.SetNewInclemency(inclemencyTypeByVail, instantly: true);
			}
			if (m_WindInclemencyController.TargetInclemency != inclemencyTypeByVail)
			{
				m_WindInclemencyController.SetNewInclemency(inclemencyTypeByVail, instantly: true);
			}
			TryFlashBrokenVeil(delta, value);
		}
	}

	private void TryFlashBrokenVeil(int delta, int value)
	{
		BlueprintBuffReference veilTurnFlashingBuff = BlueprintWarhammerRoot.Instance.WarpWeatherRoot.VeilTurnFlashingBuff;
		int veilBrokenThreshold = BlueprintWarhammerRoot.Instance.WarpWeatherRoot.VeilBrokenThreshold;
		if (delta > 0 && value - delta < veilBrokenThreshold && value >= veilBrokenThreshold)
		{
			GameHelper.ApplyBuff(GameHelper.GetPlayerCharacter(), veilTurnFlashingBuff)?.AddSource(GameHelper.GetPlayerCharacter());
		}
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if ((Game.Instance.CurrentlyLoadedAreaPart ?? Game.Instance.CurrentlyLoadedArea).VailAffectsTheWeather && BlueprintWarhammerRoot.Instance.WarpWeatherRoot != null)
		{
			InclemencyType inclemencyType;
			InclemencyType inclemencyType2;
			if (inCombat)
			{
				m_WeatherInclemencyController.SetNewInclemency(InclemencyType.Clear, instantly: true);
				inclemencyType = InclemencyType.Clear;
				inclemencyType2 = InclemencyType.Clear;
			}
			else
			{
				InclemencyType inclemencyTypeByVail = BlueprintWarhammerRoot.Instance.WarpWeatherRoot.GetInclemencyTypeByVail(Game.Instance.TurnController.VeilThicknessCounter.Value);
				inclemencyType = ((!VFXWeatherSystem.Instance.IsProfileOverriden || !Game.Instance.Player.Weather.CurrentWeatherEffect.HasValue) ? inclemencyTypeByVail : Game.Instance.Player.Weather.CurrentWeatherEffect.Value);
				inclemencyType2 = inclemencyTypeByVail;
			}
			if (m_WeatherInclemencyController.TargetInclemency != inclemencyType)
			{
				m_WeatherInclemencyController.SetNewInclemency(inclemencyType, instantly: true);
			}
			if (m_WindInclemencyController.TargetInclemency != inclemencyType2)
			{
				m_WindInclemencyController.SetNewInclemency(inclemencyType2, instantly: true);
			}
		}
	}

	public void SetWeatherEffectInclemency(InclemencyType type, bool instantly, float changeSpeed)
	{
		m_WeatherInclemencyController?.SetNewInclemency(type, instantly, changeSpeed);
	}

	public void SetWeatherVisualEffect(InclemencyType inclemencyType, VisualStateEffectType effectType)
	{
		Game.Instance.Player.Weather.CurrentWeatherEffect = inclemencyType;
		Game.Instance.Player.Weather.VisualStateEffectType = effectType;
		OverrideWeatherProfile(effectType);
	}

	private void OverrideWeatherProfile(VisualStateEffectType effectType)
	{
		if (BlueprintWarhammerRoot.Instance.WarpWeatherRoot.TryGetWeatherEffectProfile(effectType, out var weatherProfile))
		{
			VFXWeatherSystem.Instance.OverrideProfile(weatherProfile != null, weatherProfile);
		}
	}

	public void ResetWeatherVisualEffect(float changeSpeed)
	{
		Game.Instance.Player.Weather.CurrentWeatherEffect = null;
		Game.Instance.Player.Weather.VisualStateEffectType = null;
		VFXWeatherSystem.Instance.OverrideProfile(overrideProfile: false, null);
		SetWeatherEffectInclemency(Game.Instance.Player.Weather.CurrentWeather, instantly: false, changeSpeed);
	}
}
