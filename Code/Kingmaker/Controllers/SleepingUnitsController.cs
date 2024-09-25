using System.Collections.Generic;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers;

public class SleepingUnitsController : IControllerTick, IController, IControllerStart, IControllerStop, IAreaHandler, ISubscriber
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void OnStart()
	{
		Tick();
	}

	public void OnStop()
	{
		Game.Instance.State.ClearAwakeUnits();
	}

	public void OnAreaBeginUnloading()
	{
		Game.Instance.State.ClearAwakeUnits();
	}

	public void OnAreaDidLoad()
	{
	}

	public void Tick()
	{
		using (ProfileScope.New("Units"))
		{
			List<AbstractUnitEntity> list = ListPool<AbstractUnitEntity>.Claim();
			foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
			{
				using (ProfileScope.New("Update dead bodies"))
				{
					if (allUnit.LifeState.IsFinallyDead)
					{
						allUnit.LifeState.IsDeathRevealed = allUnit.IsInCameraFrustum && allUnit.IsVisibleForPlayer;
					}
				}
				allUnit.IsSleeping = ShouldBeSleeping(allUnit);
				if (!allUnit.IsSleeping)
				{
					list.Add(allUnit);
				}
			}
			Game.Instance.State.SetNewAwakeUnits(list);
			ListPool<AbstractUnitEntity>.Release(list);
		}
		using (ProfileScope.New("Groups"))
		{
			List<UnitGroup> readyForCombatUnitGroups = Game.Instance.ReadyForCombatUnitGroups;
			readyForCombatUnitGroups.Clear();
			foreach (UnitGroup unitGroup in Game.Instance.UnitGroups)
			{
				for (int i = 0; i < unitGroup.Count; i++)
				{
					BaseUnitEntity baseUnitEntity = unitGroup[i];
					if (baseUnitEntity != null && !baseUnitEntity.IsSleeping && !baseUnitEntity.Faction.Peaceful && (!baseUnitEntity.IsInFogOfWar || baseUnitEntity.IsInCombat || baseUnitEntity.AwakeTimer > 0f))
					{
						readyForCombatUnitGroups.Add(unitGroup);
						break;
					}
				}
			}
		}
	}

	private static bool ShouldBeSleeping(AbstractUnitEntity unit)
	{
		if (!unit.IsInGame || unit.Blueprint.IsFake || (unit.Suppressed && !unit.IsInCombat))
		{
			return true;
		}
		if (unit.AwakeTimer >= 0f)
		{
			unit.AwakeTimer -= Game.Instance.TimeController.DeltaTime;
			return false;
		}
		if ((bool)unit.Sleepless || !CutsceneControlledUnit.IsSleepingAllowed(unit))
		{
			return false;
		}
		if (unit.IsExtra && !unit.IsDead && (unit.IsInFogOfWar || !unit.IsInCameraFrustum))
		{
			return true;
		}
		if (unit.IsInFogOfWar && !unit.IsInCombat)
		{
			return unit.Commands.Empty;
		}
		return false;
	}
}
