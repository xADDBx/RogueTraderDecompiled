using System;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics.Entities;

namespace Kingmaker.Controllers.Units;

public class UnitMoveControllerLate : IControllerTick, IController
{
	private readonly TimeSpan m_MinDeltaTime = 0.001f.Seconds();

	public TickType GetTickType()
	{
		return TickType.EndOfFrame;
	}

	public void Tick()
	{
		MovableEntitiesEnumerable.Enumerator enumerator = default(MovableEntitiesEnumerable).GetEnumerator();
		while (enumerator.MoveNext())
		{
			AbstractUnitEntity current = enumerator.Current;
			TickUnit(current);
		}
	}

	private void TickUnit([NotNull] AbstractUnitEntity unit)
	{
		AbstractUnitEntityView view = unit.View;
		if (!(view == null))
		{
			view.InterpolationHelper.Interpolate(Game.Instance.RealTimeController.InterpolationProgress);
		}
	}
}
