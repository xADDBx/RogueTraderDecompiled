using System;
using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.Timer;

public class TimerController : IControllerEnable, IController, IControllerTick, ITimerHandler, ISubscriber
{
	private readonly List<(ITimer timer, TimeSpan runningTime)> m_Timers = new List<(ITimer, TimeSpan)>();

	public void OnEnable()
	{
		m_Timers.Clear();
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		for (int num = m_Timers.Count - 1; num >= 0; num--)
		{
			if (!(m_Timers[num].runningTime > Game.Instance.TimeController.GameTime))
			{
				try
				{
					m_Timers[num].timer.RunCallback();
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
				finally
				{
					m_Timers.RemoveAt(num);
				}
			}
		}
	}

	public void SubscribeTimer(ITimer timer)
	{
		m_Timers.Add((timer, Game.Instance.TimeController.GameTime + timer.TimerTime));
	}
}
