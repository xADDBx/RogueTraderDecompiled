using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Controllers.Units;

public class OverwatchController : IControllerTick, IController, ITurnBasedModeHandler, ISubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>, IUnitCommandStartHandler, IUnitCommandActHandler, IEntityPositionChangedHandler, ISubscriber<IEntity>
{
	private readonly HashSet<BaseUnitEntity> m_TriggersUpdateList = new HashSet<BaseUnitEntity>();

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (!TurnController.IsInTurnBasedCombat())
		{
			m_TriggersUpdateList.Clear();
		}
		if (m_TriggersUpdateList.Empty())
		{
			return;
		}
		try
		{
			foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
			{
				PartOverwatch optional = allBaseAwakeUnit.GetOptional<PartOverwatch>();
				if (optional == null)
				{
					continue;
				}
				foreach (BaseUnitEntity triggersUpdate in m_TriggersUpdateList)
				{
					if (optional.IsStopped)
					{
						break;
					}
					if (allBaseAwakeUnit.IsEnemy(triggersUpdate) && optional.Contains(triggersUpdate))
					{
						optional.TryTriggerAttack(triggersUpdate);
					}
				}
			}
		}
		finally
		{
			m_TriggersUpdateList.Clear();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		StopOverwatching(EventInvokerExtensions.MechanicEntity);
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			return;
		}
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			allUnit.GetOptional<PartOverwatch>()?.Stop();
		}
		m_TriggersUpdateList.Clear();
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (!(command is UnitOverwatchAttack))
		{
			StopOverwatching(command.Executor);
		}
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		if (IsTriggersOverwatch(command) && command.Executor is BaseUnitEntity item)
		{
			m_TriggersUpdateList.Add(item);
		}
	}

	public void HandleEntityPositionChanged()
	{
		if (EventInvokerExtensions.Entity is BaseUnitEntity item)
		{
			m_TriggersUpdateList.Add(item);
		}
	}

	private static bool IsTriggersOverwatch(AbstractUnitCommand cmd)
	{
		UnitUseAbility obj = cmd as UnitUseAbility;
		if (obj == null)
		{
			return false;
		}
		return obj.Ability.Blueprint.UsingInOverwatchArea == BlueprintAbility.UsingInOverwatchAreaType.WillCauseAttack;
	}

	private static void StopOverwatching(MechanicEntity unit)
	{
		unit.Parts.GetOptional<PartOverwatch>()?.Stop();
	}
}
