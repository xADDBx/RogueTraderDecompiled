using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

public static class InvisibleHelper
{
	public static bool Has(this RevealReason reason, RevealReason flag)
	{
		return (reason & flag) != 0;
	}

	public static void ProcessMoveStart(this BaseUnitEntity unit, ForcedPath path, bool interruptPlayerMovement, Action<ForcedPath> updatePathCallback = null)
	{
		if (TurnController.IsInTurnBasedCombat() && unit != null)
		{
			TryMarkRevealedSelf(path, unit, interruptPlayerMovement);
			TryMarkRevealedEnemies(path, unit, interruptPlayerMovement, out var newPath);
			if (newPath != null)
			{
				updatePathCallback?.Invoke(newPath);
			}
		}
	}

	public static void ProcessMoveTick(this AbstractUnitEntity unit)
	{
		if (TurnController.IsInTurnBasedCombat() && unit is BaseUnitEntity baseUnitEntity)
		{
			TryReveal(baseUnitEntity, baseUnitEntity);
			TryRevealEnemies(baseUnitEntity);
		}
	}

	public static void ProcessMoveEnd(this BaseUnitEntity unit)
	{
		if (TurnController.IsInTurnBasedCombat() && unit != null)
		{
			unit.GetOptional<PartUnitInvisibleInCombat>()?.ClearRevealSources();
			GetEnemies(unit).ForEach(delegate(BaseUnitEntity entity)
			{
				entity.GetOptional<PartUnitInvisibleInCombat>()?.ClearRevealSources();
			});
		}
	}

	private static void TryMarkRevealedSelf(ForcedPath path, BaseUnitEntity invoker, bool interruptPlayerMovement)
	{
		PartUnitInvisibleInCombat optional = invoker.GetOptional<PartUnitInvisibleInCombat>();
		if (optional == null || !optional.RevealReason.Has(RevealReason.Movement))
		{
			return;
		}
		List<BaseUnitEntity> enemies = GetEnemies(invoker);
		for (int i = 0; i < path.vectorPath.Count; i++)
		{
			Vector3 position = path.vectorPath[i];
			for (int j = 0; j < enemies.Count; j++)
			{
				BaseUnitEntity baseUnitEntity = enemies[j];
				if (baseUnitEntity.CanReveal(invoker, position.GetNearestNodeXZ(), optional.RevealRadius))
				{
					optional.AddRevealSource(baseUnitEntity, interruptPlayerMovement);
				}
			}
		}
	}

	private static void TryMarkRevealedEnemies(ForcedPath path, BaseUnitEntity invoker, bool interruptPlayerMovement, out ForcedPath newPath)
	{
		List<BaseUnitEntity> enemies = GetEnemies(invoker);
		newPath = null;
		for (int i = 0; i < path.path.Count; i++)
		{
			GraphNode graphNode = path.path[i];
			for (int j = 0; j < enemies.Count; j++)
			{
				BaseUnitEntity baseUnitEntity = enemies[j];
				PartUnitInvisibleInCombat optional = baseUnitEntity.GetOptional<PartUnitInvisibleInCombat>();
				if (optional != null && optional.RevealReason.Has(RevealReason.Movement) && baseUnitEntity.CanReveal(invoker, graphNode.Vector3Position.GetNearestNodeXZ(), optional.RevealRadius))
				{
					optional.AddRevealSource(invoker, interruptPlayerMovement);
					if (newPath == null)
					{
						newPath = ForcedPath.Construct(path.path.Take(i + 1));
					}
				}
			}
		}
	}

	private static void TryReveal(BaseUnitEntity invisibleUnit, BaseUnitEntity movingUnit)
	{
		PartUnitInvisibleInCombat optional = invisibleUnit.GetOptional<PartUnitInvisibleInCombat>();
		if (optional == null)
		{
			return;
		}
		for (int i = 0; i < optional.RevealDataList.Count; i++)
		{
			InvisibleRevealData invisibleRevealData = optional.RevealDataList[i];
			if (invisibleRevealData.EntityRef.Entity is BaseUnitEntity entity && entity.DistanceToInCells(invisibleUnit) <= optional.RevealRadius)
			{
				if (invisibleRevealData.InterruptPlayerMovement && movingUnit.IsInPlayerParty)
				{
					movingUnit.GetOptional<PartUnitCommands>()?.ForceInterruptMove();
				}
				optional.SourceBuff.Remove();
				Game.Instance.CameraController?.Follower?.ScrollTo(invisibleUnit);
				break;
			}
		}
	}

	private static void TryRevealEnemies(BaseUnitEntity self)
	{
		List<BaseUnitEntity> enemies = GetEnemies(self);
		for (int i = 0; i < enemies.Count; i++)
		{
			TryReveal(enemies[i], self);
		}
	}

	private static List<BaseUnitEntity> GetEnemies(BaseUnitEntity invoker)
	{
		List<BaseUnitEntity> allBaseAwakeUnits = Game.Instance.State.AllBaseAwakeUnits;
		List<BaseUnitEntity> list = TempList.Get<BaseUnitEntity>();
		for (int i = 0; i < allBaseAwakeUnits.Count; i++)
		{
			BaseUnitEntity baseUnitEntity = allBaseAwakeUnits[i];
			if (baseUnitEntity.IsEnemy(invoker))
			{
				list.Add(baseUnitEntity);
			}
		}
		return list;
	}

	private static bool CanReveal(this BaseUnitEntity staticUnit, BaseUnitEntity target, CustomGridNodeBase origin, int revealRadius)
	{
		bool num = staticUnit.DistanceToInCells(origin.Vector3Position, target.SizeRect) <= revealRadius;
		bool flag = LosCalculations.HasLos(origin, staticUnit.SizeRect, target.CurrentUnwalkableNode, target.SizeRect);
		return num && flag;
	}
}
