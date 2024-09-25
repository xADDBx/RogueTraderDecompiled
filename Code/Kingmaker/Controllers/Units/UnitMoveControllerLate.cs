using System;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics.Entities;

namespace Kingmaker.Controllers.Units;

public class UnitMoveControllerLate : IControllerTick, IController, IControllerDisable
{
	private readonly TimeSpan m_MinDeltaTime = 0.001f.Seconds();

	public TickType GetTickType()
	{
		return TickType.EndOfFrame;
	}

	public void Tick()
	{
		float interpolationProgress = Game.Instance.RealTimeController.InterpolationProgress;
		MovableEntitiesEnumerable.Enumerator enumerator = default(MovableEntitiesEnumerable).GetEnumerator();
		while (enumerator.MoveNext())
		{
			AbstractUnitEntity current = enumerator.Current;
			TickUnit(current, interpolationProgress);
		}
	}

	void IControllerDisable.OnDisable()
	{
		MovableEntitiesEnumerable.Enumerator enumerator = default(MovableEntitiesEnumerable).GetEnumerator();
		while (enumerator.MoveNext())
		{
			AbstractUnitEntity current = enumerator.Current;
			TickUnit(current, 1f);
		}
	}

	private void TickUnit([NotNull] AbstractUnitEntity unit, float progress)
	{
		AbstractUnitEntityView view = unit.View;
		if (!(view == null))
		{
			view.InterpolationHelper.Interpolate(progress);
		}
	}
}
