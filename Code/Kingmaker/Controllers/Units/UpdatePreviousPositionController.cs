using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UpdatePreviousPositionController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		MovableEntitiesEnumerable.Enumerator enumerator = default(MovableEntitiesEnumerable).GetEnumerator();
		while (enumerator.MoveNext())
		{
			AbstractUnitEntity current = enumerator.Current;
			try
			{
				ProcessUnit(current);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
		static void ProcessUnit(AbstractUnitEntity unit)
		{
			if (!(unit.View == null))
			{
				unit.View.InterpolationHelper.OnUnitSimulationTickCompleted(unit.Features.OnElevator);
				unit.Movable.PreviousSimulationTick = new PartMovable.PreviousSimulationTickInfo
				{
					HasMotion = (unit.Movable.HasMotionThisSimulationTick || unit.Movable.ForceHasMotion),
					HasRotation = !Mathf.Approximately(unit.Orientation, unit.Movable.PreviousOrientation)
				};
				unit.Movable.ForceHasMotion = false;
				unit.Movable.PreviousPosition = unit.Position;
				unit.Movable.PreviousOrientation = unit.Orientation;
			}
		}
	}
}
