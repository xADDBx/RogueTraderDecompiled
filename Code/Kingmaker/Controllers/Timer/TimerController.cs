using System;
using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.Timer;

public class TimerController : IControllerEnable, IController, IControllerTick, ITimerHandler, ISubscriber
{
	private readonly List<ITimer> m_Timers = new List<ITimer>();

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
			try
			{
				if (m_Timers[num].Tick() && m_Timers.Count > num)
				{
					m_Timers.RemoveAt(num);
				}
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
				if (m_Timers.Count > num)
				{
					m_Timers.RemoveAt(num);
				}
			}
		}
	}

	public void SubscribeTimer(ITimer timer)
	{
		m_Timers.Add(timer);
	}

	public void CancelTimer(ITimer timer)
	{
		m_Timers.Remove(timer);
	}
}
