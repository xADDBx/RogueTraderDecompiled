using System;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.Units;

public class InvisibleUnitController : IController, IUnitMovementHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	public void HandleWaypointUpdate(int index)
	{
	}

	public void HandleMovementComplete()
	{
		bool flag = false;
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
		{
			if (!allBaseAwakeUnit.IsInCombat || !allBaseAwakeUnit.CombatGroup.IsEnemy(baseUnitEntity) || !(SizePathfindingHelper.BoundsDistance(baseUnitEntity, allBaseAwakeUnit) <= 0.1f))
			{
				continue;
			}
			if (allBaseAwakeUnit.IsInvisible)
			{
				EventBus.RaiseEvent((IMechanicEntity)allBaseAwakeUnit, (Action<IUnitInvisibleHandler>)delegate(IUnitInvisibleHandler e)
				{
					e.RemoveUnitInvisible();
				}, isCheckRuntime: true);
			}
			flag = true;
		}
		if (flag && baseUnitEntity.IsInvisible)
		{
			EventBus.RaiseEvent((IMechanicEntity)baseUnitEntity, (Action<IUnitInvisibleHandler>)delegate(IUnitInvisibleHandler e)
			{
				e.RemoveUnitInvisible();
			}, isCheckRuntime: true);
		}
	}
}
