using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Controllers;

public class UpdateController<T> : IControllerTick, IController, IControllerReset where T : IUpdatable
{
	private readonly TickType m_TickType;

	private readonly UpdatableQueue<T> m_Updatables = new UpdatableQueue<T>();

	private UpdateController()
	{
	}

	public UpdateController(TickType tickType)
	{
		m_TickType = tickType;
	}

	public void Add(T updatable)
	{
		m_Updatables.Add(updatable);
	}

	public void Remove(T updatable)
	{
		m_Updatables.Remove(updatable);
	}

	TickType IControllerTick.GetTickType()
	{
		return m_TickType;
	}

	void IControllerTick.Tick()
	{
		float delta;
		switch (m_TickType)
		{
		case TickType.BeginOfFrame:
		case TickType.EndOfFrame:
			delta = Time.deltaTime;
			break;
		case TickType.Simulation:
			delta = Game.Instance.TimeController.DeltaTime;
			break;
		case TickType.Network:
			delta = Game.Instance.RealTimeController.NetworkDeltaTime;
			break;
		case TickType.System:
			delta = Game.Instance.RealTimeController.SystemDeltaTime;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		m_Updatables.Prepare();
		T value;
		while (m_Updatables.Next(out value))
		{
			try
			{
				value.Tick(delta);
			}
			catch (Exception ex)
			{
				PFLog.Tick.Exception(ex);
			}
		}
	}

	void IControllerReset.OnReset()
	{
		m_Updatables.Clear();
	}
}
