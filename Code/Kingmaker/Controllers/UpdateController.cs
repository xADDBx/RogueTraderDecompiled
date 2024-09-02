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

	public void AddUnique(T updatable)
	{
		if (!Contains(updatable))
		{
			Add(updatable);
		}
	}

	public void Remove(T updatable)
	{
		m_Updatables.Remove(updatable);
	}

	public bool Contains(T updatable)
	{
		return m_Updatables.Contains(updatable);
	}

	public bool TryFind(Predicate<T> predicate, out T result)
	{
		return m_Updatables.TryFind(predicate, out result);
	}

	TickType IControllerTick.GetTickType()
	{
		return m_TickType;
	}

	void IControllerTick.Tick()
	{
		float deltaTime;
		switch (m_TickType)
		{
		case TickType.BeginOfFrame:
		case TickType.EndOfFrame:
			deltaTime = Time.deltaTime;
			break;
		case TickType.Simulation:
			deltaTime = Game.Instance.TimeController.DeltaTime;
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
				value.Tick(deltaTime);
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
