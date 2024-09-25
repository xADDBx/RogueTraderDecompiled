using System;
using Code.GameCore.Mics;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Controllers;

public class TimeController : IControllerEnable, IController, IControllerDisable, IControllerTick, IControllerStop, ITimeController, InterfaceService
{
	private TimeSpan m_DeltaTime;

	private TimeSpan m_GameDeltaTime;

	private float m_PlayerTimeScale = 1f;

	private float m_GameTimeScale = 1f;

	private float m_CameraFollowTimeScale = 1f;

	private int m_CameraFollowTimeScaleLastTick;

	private float m_SlowMoTimeScale = 1f;

	public float DebugTimeScale = 1f;

	private const long SimulationStepMs = 50L;

	private int m_LastSimulationTick;

	public TimeSpan DeltaTimeSpan => m_DeltaTime;

	public float DeltaTime => (float)m_DeltaTime.TotalSeconds;

	public TimeSpan GameDeltaTimeSpan => m_GameDeltaTime;

	public float GameDeltaTime => (float)m_GameDeltaTime.TotalSeconds;

	public TimeSpan GameDeltaTimeInterpolation => Game.Instance.RealTimeController.DeltaTimeInterpolation * TimeScale;

	public float PlayerTimeScale
	{
		get
		{
			return m_PlayerTimeScale;
		}
		set
		{
			if (m_PlayerTimeScale != value)
			{
				m_PlayerTimeScale = value;
			}
		}
	}

	public float GameTimeScale
	{
		get
		{
			return m_GameTimeScale;
		}
		set
		{
			if (m_GameTimeScale != value)
			{
				m_GameTimeScale = value;
			}
		}
	}

	public float CameraFollowTimeScale
	{
		get
		{
			return m_CameraFollowTimeScale;
		}
		private set
		{
			if (m_CameraFollowTimeScale != value)
			{
				m_CameraFollowTimeScale = value;
			}
		}
	}

	public float SlowMoTimeScale
	{
		get
		{
			return m_SlowMoTimeScale;
		}
		set
		{
			if (m_SlowMoTimeScale != value)
			{
				m_SlowMoTimeScale = value;
			}
		}
	}

	public float TimeScale => PlayerTimeScale * CameraFollowTimeScale * DebugTimeScale * SlowMoTimeScale;

	public TimeSpan GameTime => Game.Instance.Player.GameTime;

	public TimeSpan RealTime => Game.Instance.Player.RealTime;

	public bool IsGameDeltaTimeZero
	{
		get
		{
			if (Game.Instance.CurrentMode == GameModeType.Default || Game.Instance.CurrentMode == GameModeType.SpaceCombat || Game.Instance.CurrentMode == GameModeType.Cutscene)
			{
				return Mathf.Approximately(Time.timeScale, 0f);
			}
			if (Game.Instance.CurrentMode == GameModeType.StarSystem || Game.Instance.CurrentMode == GameModeType.GlobalMap)
			{
				if (!Mathf.Approximately(Time.timeScale, 0f))
				{
					return Mathf.Approximately(GameTimeScale, 0f);
				}
				return true;
			}
			return true;
		}
	}

	private static TimeSpan SimulationStepTimeSpan => TimeSpan.FromTicks(500000L);

	public bool IsSimulationTick
	{
		get
		{
			if (Game.Instance.RealTimeController.IsSimulationTick)
			{
				return m_LastSimulationTick == Game.Instance.RealTimeController.CurrentSystemStepIndex;
			}
			return false;
		}
	}

	public void SetCameraFollowTimeScale(float value, bool force)
	{
		int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
		bool flag = m_CameraFollowTimeScaleLastTick != currentNetworkTick;
		m_CameraFollowTimeScaleLastTick = currentNetworkTick;
		if (!force)
		{
			value = (flag ? value : Mathf.Min(m_CameraFollowTimeScale, value));
		}
		CameraFollowTimeScale = value;
	}

	public bool CanTick(IControllerTick controller)
	{
		return controller.GetTickType() switch
		{
			TickType.Any => true, 
			TickType.BeginOfFrame => Game.Instance.RealTimeController.IsBeginFrameTick, 
			TickType.EndOfFrame => Game.Instance.RealTimeController.IsEndFrameTick, 
			TickType.Simulation => Game.Instance.RealTimeController.IsSimulationTick, 
			TickType.None => false, 
			_ => throw new ArgumentOutOfRangeException($"controller.GetTickType() = {controller.GetTickType()}"), 
		};
	}

	public TickType GetTickType()
	{
		return TickType.Any;
	}

	public void Tick()
	{
		float timeScale = TimeScale;
		if (!Game.Instance.RealTimeController.IsSimulationTick)
		{
			m_DeltaTime = TimeSpan.Zero;
			m_GameDeltaTime = TimeSpan.Zero;
		}
		else
		{
			TimeSpan systemDeltaTimeSpan = Game.Instance.RealTimeController.SystemDeltaTimeSpan;
			systemDeltaTimeSpan *= (double)timeScale;
			m_DeltaTime = systemDeltaTimeSpan;
			if (Game.Instance.CurrentMode == GameModeType.Default || Game.Instance.CurrentMode == GameModeType.SpaceCombat || Game.Instance.CurrentMode == GameModeType.Cutscene)
			{
				m_GameDeltaTime = m_DeltaTime;
				Game.Instance.Player.GameTime += m_GameDeltaTime;
				EventBus.RaiseEvent(delegate(IGameTimeChangedHandler h)
				{
					h.HandleGameTimeChanged(m_GameDeltaTime);
				});
			}
			else if (Game.Instance.CurrentMode == GameModeType.StarSystem || Game.Instance.CurrentMode == GameModeType.GlobalMap)
			{
				m_GameDeltaTime = m_DeltaTime * GameTimeScale;
				Game.Instance.Player.GameTime += m_GameDeltaTime;
				EventBus.RaiseEvent(delegate(IGameTimeChangedHandler h)
				{
					h.HandleGameTimeChanged(m_GameDeltaTime);
				});
			}
			else
			{
				m_GameDeltaTime = m_DeltaTime;
				EventBus.RaiseEvent(delegate(IGameTimeChangedHandler h)
				{
					h.HandleNonGameTimeChanged();
				});
			}
			m_LastSimulationTick = Game.Instance.RealTimeController.CurrentSystemStepIndex;
		}
		Time.timeScale = timeScale;
	}

	public void OnEnable()
	{
		Time.timeScale = PlayerTimeScale;
	}

	public void OnDisable()
	{
		Suspend();
	}

	public void Suspend()
	{
		Time.timeScale = 0f;
		m_DeltaTime = TimeSpan.Zero;
		m_GameDeltaTime = TimeSpan.Zero;
	}

	public TimeSpan SkipGameTime(TimeOfDay timeOfDay)
	{
		if (Game.Instance.TimeOfDay == timeOfDay)
		{
			return TimeSpan.Zero;
		}
		int hours = GameTime.Hours;
		int hours2 = timeOfDay.Time().Hours;
		TimeSpan timeSpan = ((hours < hours2) ? (hours2 - hours) : (hours2 + 24 - hours)).Hours() - GameTime.Minutes.Minutes() - GameTime.Seconds.Seconds();
		AdvanceGameTime(timeSpan);
		return timeSpan;
	}

	public void AdvanceGameTime(TimeSpan delta)
	{
		Game.Instance.Player.GameTime += delta;
		EventBus.RaiseEvent(delegate(IGameTimeChangedHandler h)
		{
			h.HandleGameTimeChanged(delta);
		});
		EventBus.RaiseEvent(delegate(IGameTimeAdvancedHandler h)
		{
			h.HandleGameTimeAdvanced(delta);
		});
	}

	public void OnStop()
	{
		Time.timeScale = 1f;
		m_DeltaTime = TimeSpan.Zero;
		m_GameDeltaTime = TimeSpan.Zero;
		m_PlayerTimeScale = 1f;
		m_GameTimeScale = 1f;
		m_CameraFollowTimeScale = 1f;
		m_CameraFollowTimeScaleLastTick = 0;
		m_SlowMoTimeScale = 1f;
		DebugTimeScale = 1f;
		m_LastSimulationTick = -1;
	}

	public void SetDeltaTime(float value)
	{
		m_DeltaTime = value.Seconds();
	}

	public void SetGameDeltaTime(float value)
	{
		m_GameDeltaTime = value.Seconds();
	}
}
