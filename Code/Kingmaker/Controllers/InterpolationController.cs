using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Controllers;

public class InterpolationController<T> : IControllerTick, IController where T : IInterpolatable
{
	private readonly UpdatableQueue<T> m_InterpolatableList = new UpdatableQueue<T>();

	public void Add(T updatable)
	{
		m_InterpolatableList.Add(updatable);
	}

	public void AddUnique(T updatable)
	{
		if (!m_InterpolatableList.Contains(updatable))
		{
			m_InterpolatableList.Add(updatable);
		}
	}

	public void Remove(T updatable)
	{
		m_InterpolatableList.Remove(updatable);
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.EndOfFrame;
	}

	void IControllerTick.Tick()
	{
		float interpolationProgress = Game.Instance.RealTimeController.InterpolationProgress;
		Tick(interpolationProgress);
	}

	public void Tick(float progress)
	{
		m_InterpolatableList.Prepare();
		T value;
		while (m_InterpolatableList.Next(out value))
		{
			try
			{
				value.Tick(progress);
			}
			catch (Exception ex)
			{
				PFLog.Tick.Exception(ex);
			}
		}
	}
}
