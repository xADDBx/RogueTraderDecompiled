using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers.Units;

public class BuffsController : IControllerTick, IController, IControllerEnable, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, ITurnEndHandler, IInterruptTurnStartHandler, IRoundStartHandler, IRoundEndHandler, ITurnBasedModeHandler, IFactCollectionUpdatedHandler, IExitSpaceCombatHandler, IAreaHandler
{
	[CanBeNull]
	private EntityFactsManager m_CurrentManager;

	private bool m_FactRemovedWhileTick;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void OnEnable()
	{
		RemoveInvalidBuffs();
	}

	public void Tick()
	{
		List<Buff> list = TempList.Get<Buff>();
		foreach (Buff item in EntityFactService.Instance.Get<Buff>())
		{
			PartLifeState lifeStateOptional = item.Owner.GetLifeStateOptional();
			bool flag = lifeStateOptional != null && !lifeStateOptional.IsConscious;
			if (item.IsExpired || (flag && !item.Blueprint.StayOnDeath))
			{
				list.Add(item);
			}
		}
		foreach (Buff item2 in list)
		{
			item2.Remove();
		}
	}

	private void TickBuffs(bool isTurnBased, Initiative.Event @event)
	{
		if (!isTurnBased && @event != 0)
		{
			return;
		}
		foreach (MechanicEntity mechanicEntity in Game.Instance.State.MechanicEntities)
		{
			TickBuffsOnEntity(mechanicEntity, isTurnBased, @event);
		}
	}

	private void TickBuffsOnEntity(MechanicEntity entity, bool isTurnBased, Initiative.Event @event, int depth = 0)
	{
		if (entity is BaseUnitEntity { IsExtra: not false } || !entity.IsInGame)
		{
			return;
		}
		List<Buff> list = TempList.Get<Buff>();
		try
		{
			m_CurrentManager = entity.Facts;
			List<EntityFact> list2 = entity.Facts.List;
			for (int j = 0; j < list2.Count; j++)
			{
				EntityFact entityFact = list2[j];
				if (!(entityFact is Buff buff))
				{
					if (entityFact.Active && @event == Initiative.Event.RoundEnd)
					{
						entityFact.CallComponents(delegate(ITickEachRound i)
						{
							i.OnNewRound();
						});
					}
					continue;
				}
				if (buff.Active && buff.Initiative.ShouldActNow(isTurnBased, @event, out var actRound))
				{
					buff.Initiative.LastTurn = actRound;
					buff.NextRound();
				}
				if (buff.IsExpired)
				{
					list.Add(buff);
				}
			}
		}
		finally
		{
			m_CurrentManager = null;
			m_FactRemovedWhileTick = false;
		}
		foreach (Buff item in list)
		{
			item.Remove();
		}
		if (m_FactRemovedWhileTick)
		{
			if (depth > 100)
			{
				throw new Exception("TickBuffsOnEntity: too deep recursive call, something wrong");
			}
			TickBuffsOnEntity(entity, isTurnBased, @event, depth + 1);
		}
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		HandleBuffsWhichShouldBeDisabledOutOfCasterTurn();
		HandleBuffsWithEndCondition(EventInvokerExtensions.MechanicEntity, BuffEndCondition.TurnStartOrCombatEnd);
		TickBuffs(isTurnBased, Initiative.Event.TurnStart);
	}

	void IInterruptTurnStartHandler.HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		HandleBuffsWhichShouldBeDisabledOutOfCasterTurn();
	}

	void ITurnEndHandler.HandleUnitEndTurn(bool isTurnBased)
	{
		HandleBuffsWithEndCondition(EventInvokerExtensions.MechanicEntity, BuffEndCondition.TurnEndOrCombatEnd);
	}

	void IRoundStartHandler.HandleRoundStart(bool isTurnBased)
	{
		TickBuffs(isTurnBased, Initiative.Event.RoundStart);
	}

	void IRoundEndHandler.HandleRoundEnd(bool isTurnBased)
	{
		TickBuffs(isTurnBased, Initiative.Event.RoundEnd);
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			return;
		}
		List<Buff> list = TempList.Get<Buff>();
		foreach (Buff item in EntityFactService.Instance.Get<Buff>())
		{
			BuffEndCondition endCondition = item.EndCondition;
			if (endCondition == BuffEndCondition.CombatEnd || endCondition == BuffEndCondition.TurnEndOrCombatEnd || endCondition == BuffEndCondition.TurnStartOrCombatEnd)
			{
				list.Add(item);
			}
		}
		foreach (Buff item2 in list)
		{
			item2.Remove();
		}
	}

	void IFactCollectionUpdatedHandler.HandleFactCollectionUpdated(EntityFactsProcessor collection)
	{
		if (m_CurrentManager != null)
		{
			m_FactRemovedWhileTick |= collection.Manager == m_CurrentManager;
		}
	}

	void IExitSpaceCombatHandler.HandleExitSpaceCombat()
	{
		List<Buff> list = TempList.Get<Buff>();
		foreach (Buff item in EntityFactService.Instance.Get<Buff>())
		{
			if (item.EndCondition == BuffEndCondition.SpaceCombatExit)
			{
				list.Add(item);
			}
		}
		foreach (Buff item2 in list)
		{
			item2.Remove();
		}
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
	}

	void IAreaHandler.OnAreaDidLoad()
	{
		foreach (Buff item in EntityFactService.Instance.Get<Buff>())
		{
			if (item.IsEnabled && item.GetComponent<BuffSpawnFx>() != null)
			{
				item.SpawnFxFromBuffComponent();
			}
		}
	}

	private static void HandleBuffsWithEndCondition(MechanicEntity unit, BuffEndCondition endCondition)
	{
		List<Buff> list = TempList.Get<Buff>();
		foreach (Buff item in EntityFactService.Instance.Get<Buff>())
		{
			MechanicEntity mechanicEntity = item.Context.MaybeCaster ?? item.Owner;
			if (unit == mechanicEntity && item.EndCondition == endCondition)
			{
				list.Add(item);
			}
		}
		foreach (Buff item2 in list)
		{
			item2.Remove();
		}
	}

	private static void RemoveInvalidBuffs()
	{
		List<Buff> list = TempList.Get<Buff>();
		foreach (AbstractUnitEntity item in Game.Instance.State.AllUnits.All)
		{
			foreach (Buff rawFact in item.Buffs.RawFacts)
			{
				if (IsInvalid(rawFact))
				{
					list.Add(rawFact);
				}
			}
		}
		foreach (Buff item2 in list)
		{
			PFLog.Default.Error($"Remove buff with missing source: {item2} (owner: {item2.Owner})");
			item2.Remove();
		}
		static bool IsInvalid(Buff buff)
		{
			if (buff.Sources.Count > 0)
			{
				return buff.Sources.AllItems((EntityFactSource i) => i.IsMissing || (i.Entity is AreaEffectEntity areaEffectEntity && areaEffectEntity.IsEnded));
			}
			return false;
		}
	}

	private static void HandleBuffsWhichShouldBeDisabledOutOfCasterTurn()
	{
		List<Buff> list = TempList.Get<Buff>();
		foreach (Buff item in EntityFactService.Instance.Get<Buff>())
		{
			if (item.ShouldBeDisabledOutOfCasterTurn)
			{
				list.Add(item);
			}
		}
		foreach (Buff item2 in list)
		{
			MechanicEntity maybeCaster = item2.Context.MaybeCaster;
			item2.DisabledBecauseOfNotCasterTurn = maybeCaster != Game.Instance.TurnController.CurrentUnit;
			item2.UpdateIsActive();
		}
	}
}
