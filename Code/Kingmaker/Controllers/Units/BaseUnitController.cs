using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Controllers.Units;

public abstract class BaseUnitController : IControllerTick, IController
{
	protected virtual bool TickSleeping => false;

	public virtual TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		BeforeTick();
		if (TickSleeping)
		{
			foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
			{
				TickUnit(allUnit);
			}
		}
		else
		{
			for (int i = 0; i < Game.Instance.State.AllAwakeUnits.Count; i++)
			{
				TickUnit(Game.Instance.State.AllAwakeUnits[i]);
			}
		}
		AfterTick();
	}

	private void TickUnit(AbstractUnitEntity unit)
	{
		if (!ShouldTickOnUnit(unit))
		{
			return;
		}
		try
		{
			TickOnUnit(unit);
		}
		catch (Exception ex)
		{
			LogChannelFactory.GetOrCreate(GetType().Name).Exception(ex, "Exception on unit {0}", unit);
		}
	}

	protected virtual void BeforeTick()
	{
	}

	protected virtual void AfterTick()
	{
	}

	protected virtual bool ShouldTickOnUnit(AbstractUnitEntity unit)
	{
		return !unit.LifeState.IsDead;
	}

	protected abstract void TickOnUnit(AbstractUnitEntity unit);
}
