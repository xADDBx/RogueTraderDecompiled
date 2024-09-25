using System;
using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Mics;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.GameModes;

public sealed class GameMode
{
	private readonly IController[] m_Controllers;

	private readonly (string name, IControllerTick controller)[] m_ControllersTick;

	private readonly (string name, IControllerEnable controller)[] m_ControllersEnable;

	private readonly (string name, IControllerDisable controller)[] m_ControllersDisable;

	private readonly (string name, IControllerStart controller)[] m_ControllersStart;

	private readonly (string name, IControllerStop controller)[] m_ControllersStop;

	public readonly GameModeType Type;

	private readonly ITimeController m_TimeController = InterfaceServiceLocator.GetService<ITimeController>();

	private bool IsEnabled { get; set; }

	public GameMode(GameModeType type, IEnumerable<IController> controllers)
	{
		Type = type;
		m_Controllers = controllers.ToArray();
		m_ControllersTick = (from v in controllers.OfType<IControllerTick>()
			select (TypesCache.GetTypeName(v.GetType()), v: v)).ToArray();
		m_ControllersEnable = (from v in controllers.OfType<IControllerEnable>()
			select (TypesCache.GetTypeName(v.GetType()), v: v)).ToArray();
		m_ControllersDisable = (from v in controllers.OfType<IControllerDisable>()
			select (TypesCache.GetTypeName(v.GetType()), v: v)).ToArray();
		m_ControllersStart = (from v in controllers.OfType<IControllerStart>()
			select (TypesCache.GetTypeName(v.GetType()), v: v)).ToArray();
		m_ControllersStop = (from v in controllers.OfType<IControllerStop>()
			select (TypesCache.GetTypeName(v.GetType()), v: v)).ToArray();
	}

	public void Tick()
	{
		if (!IsEnabled)
		{
			LogChannel.System.Error("GameMode.Tick: game mode is not active but tick");
		}
		for (int i = 0; i < m_ControllersTick.Length; i++)
		{
			if (!m_TimeController.CanTick(m_ControllersTick[i].controller))
			{
				continue;
			}
			using (ProfileScope.New(m_ControllersTick[i].name))
			{
				try
				{
					m_ControllersTick[i].controller.Tick();
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			}
			if (!IsEnabled)
			{
				break;
			}
		}
	}

	[CanBeNull]
	public TController GetController<TController>() where TController : class, IController
	{
		IController[] controllers = m_Controllers;
		for (int i = 0; i < controllers.Length; i++)
		{
			if (controllers[i] is TController result)
			{
				return result;
			}
		}
		return null;
	}

	public void OnEnable()
	{
		IsEnabled = true;
		IController[] controllers = m_Controllers;
		for (int i = 0; i < controllers.Length; i++)
		{
			EventBus.Subscribe(controllers[i]);
		}
		(string, IControllerEnable)[] controllersEnable = m_ControllersEnable;
		for (int i = 0; i < controllersEnable.Length; i++)
		{
			var (text, controllerEnable) = controllersEnable[i];
			using (ProfileScope.New(text))
			{
				try
				{
					controllerEnable.OnEnable();
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			}
		}
	}

	public void OnDisable()
	{
		IsEnabled = false;
		(string, IControllerDisable)[] controllersDisable = m_ControllersDisable;
		for (int i = 0; i < controllersDisable.Length; i++)
		{
			var (text, controllerDisable) = controllersDisable[i];
			using (ProfileScope.New(text))
			{
				try
				{
					controllerDisable.OnDisable();
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			}
		}
		IController[] controllers = m_Controllers;
		for (int i = 0; i < controllers.Length; i++)
		{
			EventBus.Unsubscribe(controllers[i]);
		}
	}

	public void OnStart([CanBeNull] GameMode prevMode)
	{
		(string, IControllerStart)[] controllersStart = m_ControllersStart;
		for (int i = 0; i < controllersStart.Length; i++)
		{
			IControllerStart item = controllersStart[i].Item2;
			try
			{
				if (prevMode == null || !prevMode.m_Controllers.Contains(item))
				{
					item.OnStart();
				}
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}

	public void OnStop([CanBeNull] GameMode nextMode)
	{
		(string, IControllerStop)[] controllersStop = m_ControllersStop;
		for (int i = 0; i < controllersStop.Length; i++)
		{
			IControllerStop item = controllersStop[i].Item2;
			try
			{
				if (nextMode == null || !nextMode.m_Controllers.HasItem(item))
				{
					item.OnStop();
				}
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}

	public override string ToString()
	{
		return $"Mode {Type}";
	}
}
