using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Controllers.Units;

public class UnitMemoryController : IControllerTick, IController, IControllerStart, IControllerStop, IUnitMakeOffensiveActionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, ITurnBasedModeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IEntityPositionChangedHandler, ISubscriber<IEntity>, IUnitChangeAttackFactionsHandler
{
	private struct UnitsPair
	{
		public BaseUnitEntity Observer;

		public BaseUnitEntity Target;
	}

	private readonly List<UnitsPair> m_UnitsForAdd = new List<UnitsPair>();

	private readonly HashSet<BaseUnitEntity> m_UpdateList = new HashSet<BaseUnitEntity>();

	public void OnStart()
	{
		m_UnitsForAdd.Clear();
		m_UpdateList.Clear();
	}

	public void OnStop()
	{
		Game.Instance.UnitGroups.ForEach(delegate(UnitGroup g)
		{
			g.Memory.Clear();
		});
		m_UnitsForAdd.Clear();
		m_UpdateList.Clear();
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		try
		{
			foreach (UnitGroup unitGroup in Game.Instance.UnitGroups)
			{
				unitGroup.Memory.Cleanup();
			}
			if (!TurnController.IsInTurnBasedCombat())
			{
				return;
			}
			foreach (UnitsPair item in m_UnitsForAdd)
			{
				if (!item.Observer.IsDisposed && !item.Target.IsDisposed && item.Observer.IsInCombat && item.Target.IsInCombat)
				{
					item.Observer.CombatGroup.Memory.Add(item.Target);
				}
			}
			if (!m_UpdateList.Empty())
			{
				foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
				{
					if (m_UpdateList.Contains(allBaseAwakeUnit) || !allBaseAwakeUnit.IsInCombat)
					{
						continue;
					}
					using (ProfileScope.New("TickAwakeUnit"))
					{
						foreach (BaseUnitEntity update in m_UpdateList)
						{
							AddToMemoryIfNecessary(allBaseAwakeUnit, update);
						}
					}
				}
			}
			foreach (BaseUnitEntity update2 in m_UpdateList)
			{
				using (ProfileScope.New("TickUnitFromUpdateList"))
				{
					TickUnit(update2);
				}
			}
		}
		finally
		{
			m_UnitsForAdd.Clear();
			m_UpdateList.Clear();
		}
	}

	private static void TickUnit(BaseUnitEntity unit)
	{
		if (unit.IsExtra)
		{
			return;
		}
		if (!unit.IsInCombat)
		{
			if (!unit.CombatGroup.IsInCombat)
			{
				unit.CombatGroup.Memory.Clear();
			}
			return;
		}
		foreach (BaseUnitEntity item in unit.Vision.CanBeInRange)
		{
			AddToMemoryIfNecessary(unit, item);
		}
	}

	private static bool ShouldBeInMemory(BaseUnitEntity unit, BaseUnitEntity target)
	{
		if (unit.IsExtra || target.IsExtra)
		{
			return false;
		}
		if (unit.CombatGroup.Memory.Find(target) == null)
		{
			if (target.IsInCombat)
			{
				return unit.HasLOS(target);
			}
			return false;
		}
		return true;
	}

	private static void AddToMemoryIfNecessary(BaseUnitEntity unit, BaseUnitEntity target)
	{
		if (ShouldBeInMemory(unit, target))
		{
			unit.CombatGroup.Memory.Add(target);
			target.CombatGroup.Memory.Add(unit);
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			return;
		}
		foreach (UnitGroup unitGroup in Game.Instance.UnitGroups)
		{
			unitGroup.Memory.Clear();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (EventInvokerExtensions.Entity is BaseUnitEntity item)
		{
			m_UpdateList.Add(item);
		}
	}

	public void AddToMemory(BaseUnitEntity observer, BaseUnitEntity target)
	{
		if (!target.Faction.Peaceful && !m_UnitsForAdd.Any((UnitsPair i) => i.Observer == observer && i.Target == target))
		{
			UnitsPair item = default(UnitsPair);
			item.Observer = observer;
			item.Target = target;
			m_UnitsForAdd.Add(item);
		}
	}

	public void TryForceMemoryPair(BaseUnitEntity observer, BaseUnitEntity target)
	{
		int num = m_UnitsForAdd.FindIndex((UnitsPair i) => i.Observer == observer && i.Target == target);
		if (num != -1)
		{
			m_UnitsForAdd.RemoveAt(num);
			observer.CombatGroup.Memory.Add(target);
		}
	}

	public void UpdateUnit(BaseUnitEntity unit)
	{
		if (unit.IsInCombat)
		{
			m_UpdateList.Add(unit);
		}
	}

	public void HandleUnitMakeOffensiveAction(BaseUnitEntity target)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity.IsInCombat && target.IsInCombat)
		{
			AddToMemory(baseUnitEntity, target);
			AddToMemory(target, baseUnitEntity);
		}
	}

	public void HandleEntityPositionChanged()
	{
		Entity entity = EventInvokerExtensions.Entity;
		if (TurnController.IsInTurnBasedCombat() && entity is BaseUnitEntity { IsInCombat: not false } baseUnitEntity)
		{
			m_UpdateList.Add(baseUnitEntity);
		}
	}

	public void HandleUnitChangeAttackFactions(MechanicEntity unit1)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (TurnController.IsInTurnBasedCombat() && baseUnitEntity != null && baseUnitEntity.IsInCombat)
		{
			m_UpdateList.Add(baseUnitEntity);
		}
	}
}
