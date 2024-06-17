using System;
using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Equipment;

namespace Kingmaker.Controllers.Units;

public class UnitHandEquipmentController : IControllerTick, IController
{
	private readonly List<UnitViewHandsEquipment> m_UnitsToUpdate = new List<UnitViewHandsEquipment>();

	private static readonly Predicate<UnitViewHandsEquipment> DidUpdate = _DidUpdate;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (!Game.Instance.IsPaused)
		{
			m_UnitsToUpdate.RemoveAll(DidUpdate);
		}
	}

	private static bool _DidUpdate(UnitViewHandsEquipment equipment)
	{
		BaseUnitEntity owner = equipment.Owner;
		UnitEntityView view = equipment.View;
		if (owner == null || owner.IsDisposed || owner.WillBeDestroyed || !view)
		{
			return true;
		}
		if (owner.LifeState.IsFinallyDead)
		{
			return true;
		}
		if (owner.IsSleeping)
		{
			return false;
		}
		if (!owner.State.CanAct)
		{
			return false;
		}
		return equipment.MatchWithCurrentCombatState();
	}

	public void ScheduleUpdate(UnitViewHandsEquipment unit)
	{
		if (!IsUpdateScheduledFor(unit))
		{
			m_UnitsToUpdate.Add(unit);
		}
	}

	public bool IsUpdateScheduledFor(UnitViewHandsEquipment unit)
	{
		return m_UnitsToUpdate.HasItem(unit);
	}
}
