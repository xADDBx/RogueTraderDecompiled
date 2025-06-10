using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("ce1f34db6f984679a99b3636f7237729")]
public class ContextActionForceNextInRound : ContextAction
{
	public override string GetCaption()
	{
		return "Force target to take next turn within current round";
	}

	protected override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			return;
		}
		MechanicEntity[] array = Game.Instance.TurnController.TurnOrder.UnitsOrder.ToArray();
		if (!array.Contains(entity))
		{
			PFLog.Actions.Error($"Failed to force next turn for {entity}. Unit is not in the UnitsOrder.");
			return;
		}
		MechanicEntity mechanicEntity = Game.Instance.TurnController.TurnOrder.CurrentUnit;
		if (mechanicEntity == null)
		{
			PFLog.Actions.Error($"Failed to force next turn for {entity}. No current unit found.");
			return;
		}
		HashSet<MechanicEntity> hashSet = Game.Instance.TurnController.TurnOrder.InterruptingTurnOrder.ToHashSet();
		if (mechanicEntity != null && !hashSet.Contains(mechanicEntity) && mechanicEntity.Initiative.IsInEndInterrupting)
		{
			hashSet.Add(mechanicEntity);
		}
		if (hashSet.Count > 0)
		{
			foreach (MechanicEntity item in Game.Instance.TurnController.TurnOrder.CurrentRoundUnitsOrder)
			{
				if (!hashSet.Contains(item))
				{
					mechanicEntity = item;
					break;
				}
			}
		}
		int num = array.IndexOf(mechanicEntity);
		if (num == array.Length - 1)
		{
			entity.Initiative.Value = array[num].Initiative.Value / 2f;
		}
		else
		{
			entity.Initiative.Value = (array[num].Initiative.Value + array[num + 1].Initiative.Value) / 2f;
		}
		entity.Initiative.WasPreparedForRound = mechanicEntity.Initiative.WasPreparedForRound - 1;
		entity.Initiative.LastTurn = mechanicEntity.Initiative.LastTurn - 1;
		EventBus.RaiseEvent(delegate(IInitiativeChangeHandler h)
		{
			h.HandleInitiativeChanged();
		});
	}
}
