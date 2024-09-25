using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View;
using TurnBased.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UnitFearController : BaseUnitController
{
	private class PanickedUnitData
	{
		public UnitCommandHandle MoveCmdHandle { get; set; }

		public float TimeWithoutEnemies { get; set; }
	}

	private readonly Dictionary<BaseUnitEntity, PanickedUnitData> m_PanickedUnitsData = new Dictionary<BaseUnitEntity, PanickedUnitData>();

	protected override bool ShouldTickOnUnit(AbstractUnitEntity entity)
	{
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return false;
		}
		if (base.ShouldTickOnUnit(baseUnitEntity))
		{
			return baseUnitEntity.State.IsPanicked;
		}
		return false;
	}

	protected override void TickOnUnit(AbstractUnitEntity entity)
	{
		BaseUnitEntity unit = entity as BaseUnitEntity;
		if (unit == null)
		{
			return;
		}
		using (TurnBasedZeroGameDeltaTime.Create())
		{
			float gameDeltaTime = Game.Instance.TimeController.GameDeltaTime;
			bool flag = unit.CombatGroup.Memory.Enemies.Any((UnitGroupMemory.UnitInfo e) => e.Unit.LifeState.IsConscious && unit.Vision.HasLOS(e.Unit));
			PartUnitState state = unit.State;
			m_PanickedUnitsData.TryGetValue(unit, out var value);
			bool isPanicked = state.IsPanicked;
			state.IsPanicked = flag || (value != null && (value.TimeWithoutEnemies += gameDeltaTime) < 2.5f);
			if (state.IsPanicked)
			{
				if (value == null)
				{
					value = new PanickedUnitData();
					m_PanickedUnitsData.Add(unit, value);
					unit.Commands.InterruptAllInterruptible();
				}
				UnitMoveTo currentMoveTo = unit.Commands.CurrentMoveTo;
				if ((currentMoveTo == null || currentMoveTo != value.MoveCmdHandle?.Cmd || currentMoveTo.IsFinished) && !TurnController.IsInTurnBasedCombat())
				{
					Vector3 pointForMove = GetPointForMove(unit);
					ForcedPath forcedPath = PathfindingService.Instance.FindPathRT_Blocking(unit.MovementAgent, pointForMove, 0.3f);
					if (!forcedPath.error)
					{
						UnitMoveToParams cmdParams = new UnitMoveToParams(forcedPath, pointForMove);
						value.MoveCmdHandle = unit.Commands.Run(cmdParams);
					}
				}
			}
			else if (isPanicked)
			{
				m_PanickedUnitsData.Remove(unit);
				unit.Commands.InterruptAllInterruptible();
			}
		}
	}

	private static Vector3 GetPointForMove(BaseUnitEntity unit)
	{
		Vector3 normalized = unit.CombatGroup.Memory.Enemies.Where((UnitGroupMemory.UnitInfo u) => u.Unit.LifeState.IsConscious).Aggregate(Vector3.zero, (Vector3 v, UnitGroupMemory.UnitInfo u) => v + (unit.Position - u.Unit.Position)).normalized;
		List<Vector3> list = new List<Vector3>();
		for (int i = -4; i < 5; i++)
		{
			float y = 15f * (float)i;
			Vector3 pos = unit.Position + Quaternion.Euler(0f, y, 0f) * (normalized * 20f);
			pos = ObstacleAnalyzer.GetNearestNode(pos).position;
			list.Add(pos);
		}
		return list.ElementAt(unit.Random.Range(0, 8));
	}
}
