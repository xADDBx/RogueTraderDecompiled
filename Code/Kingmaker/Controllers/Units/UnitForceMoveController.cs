using System;
using Core.Cheats;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UnitForceMoveController : BaseUnitController, IUnitGetAbilityPush, ISubscriber, IGlobalRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, IGlobalRulebookSubscriber
{
	private struct TransitionChecker : Linecast.ICanTransitionBetweenCells
	{
		private readonly MechanicEntity m_Unit;

		public BaseUnitEntity StoppedOnUnit;

		public TransitionChecker(MechanicEntity unit)
		{
			this = default(TransitionChecker);
			m_Unit = unit;
		}

		public bool CanTransitionBetweenCells(CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			if (!nodeTo.Walkable || !nodeFrom.ContainsConnection(nodeTo))
			{
				return false;
			}
			if (!WarhammerBlockManager.Instance.CanUnitStandOnNode(m_Unit, nodeTo))
			{
				return false;
			}
			StoppedOnUnit = nodeTo.GetUnit();
			if (StoppedOnUnit != null && !StoppedOnUnit.IsDeadOrUnconscious)
			{
				return StoppedOnUnit == m_Unit;
			}
			return true;
		}
	}

	private struct TransitionCheckerExceptUnit : Linecast.ICanTransitionBetweenCells
	{
		private readonly MechanicEntity m_Unit;

		public BaseUnitEntity StoppedOnUnit;

		public TransitionCheckerExceptUnit(MechanicEntity unit)
		{
			this = default(TransitionCheckerExceptUnit);
			m_Unit = unit;
		}

		public bool CanTransitionBetweenCells(CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			if (!nodeTo.Walkable || !nodeFrom.ContainsConnection(nodeTo))
			{
				return false;
			}
			if (!WarhammerBlockManager.Instance.CanUnitStandOnNode(m_Unit.SizeRect, nodeTo, m_Unit.MaybeMovementAgent?.Blocker))
			{
				return false;
			}
			StoppedOnUnit = nodeTo.GetUnit();
			if (StoppedOnUnit != null && !StoppedOnUnit.IsDeadOrUnconscious)
			{
				return StoppedOnUnit == m_Unit;
			}
			return true;
		}
	}

	private struct NodeCounter : Linecast.ICanTransitionBetweenCells
	{
		public int CellsRemaining { get; private set; }

		public bool CanTransitionBetweenCells(CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			int cellsRemaining = CellsRemaining + 1;
			CellsRemaining = cellsRemaining;
			return true;
		}
	}

	private void Notify(BaseUnitEntity unit)
	{
		EventBus.RaiseEvent(delegate(IUnitGetAbilityPush h)
		{
			h.HandleUnitAbilityPushDidActed(unit.CombatState.ForceMovedDistanceInCells);
		});
	}

	protected override void TickOnUnit(AbstractUnitEntity entity)
	{
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		UnitPartForceMove optional = baseUnitEntity.GetOptional<UnitPartForceMove>();
		if (optional == null)
		{
			return;
		}
		float deltaTime = Game.Instance.TimeController.DeltaTime;
		UnitPartForceMove.Chunk active = optional.Active;
		if (active == null)
		{
			baseUnitEntity.Position = baseUnitEntity.CurrentNode.position;
			baseUnitEntity.Remove<UnitPartForceMove>();
			Notify(baseUnitEntity);
			return;
		}
		if (active.IsFinishedMove)
		{
			active.LastTick = true;
			TriggerCollision(active, entity);
			return;
		}
		Vector3 vector = (active.TargetPosition - baseUnitEntity.Position).normalized * Math.Min(GeometryUtils.Distance2D(active.TargetPosition, baseUnitEntity.Position) / (active.MaxTime - active.PassedTime) * deltaTime, GeometryUtils.Distance2D(active.TargetPosition, baseUnitEntity.Position));
		Vector3 vector2 = baseUnitEntity.Position + vector;
		NNInfo nearestNode = ObstacleAnalyzer.GetNearestNode(baseUnitEntity.Position);
		bool flag = false;
		if (!active.IgnoreNavmesh && Linecast.LinecastGrid(nearestNode.node.Graph, baseUnitEntity.Position, vector2, nearestNode.node, out var hit))
		{
			flag = true;
			vector2 = hit.point;
		}
		float num = GeometryUtils.Distance2D(baseUnitEntity.Position, vector2);
		active.PassedTime += deltaTime;
		baseUnitEntity.ForceLookAt(active.TargetPosition + 2f * (baseUnitEntity.Position - active.TargetPosition));
		baseUnitEntity.Position = vector2;
		if (flag || num == 0f)
		{
			active.PassedTime = active.MaxTime;
		}
	}

	private static void TriggerCollision(UnitPartForceMove.Chunk chunk, AbstractUnitEntity entity)
	{
		if (chunk.CollisionDamageRank > 0)
		{
			Rulebook.Trigger(new RulePerformCollision(chunk.Pusher, entity, chunk.CollisionDamageRank));
			if (chunk.CollisionEntityRef.Entity != null)
			{
				Rulebook.Trigger(new RulePerformCollision(chunk.Pusher, chunk.CollisionEntityRef.Entity, chunk.CollisionDamageRank));
			}
		}
	}

	public void HandleUnitResultPush(int distanceInCells, MechanicEntity caster, MechanicEntity target, Vector3 fromPoint)
	{
		if (!target.Features.IgnoreAnyForceMove)
		{
			HandleResult(caster, target, fromPoint, distanceInCells);
		}
	}

	public void HandleUnitResultPush(int distanceInCells, Vector3 targetPoint, MechanicEntity target, MechanicEntity caster)
	{
		if (!target.Features.IgnoreAnyForceMove)
		{
			HandleResult(caster, target, targetPoint, -distanceInCells);
		}
	}

	public void HandleUnitAbilityPushDidActed(int distanceInCells)
	{
	}

	private static void HandleResult(MechanicEntity caster, MechanicEntity unit, Vector3 fromPoint, int distanceInCells)
	{
		CustomGridNodeBase nearestNodeXZ = unit.Position.GetNearestNodeXZ();
		if (nearestNodeXZ == null)
		{
			return;
		}
		PartUnitCombatState combatStateOptional = unit.GetCombatStateOptional();
		if (combatStateOptional != null)
		{
			combatStateOptional.ForceMovedDistanceInCells = distanceInCells;
		}
		Vector3 vector = unit.Position + (unit.Position - fromPoint).normalized * ((float)distanceInCells * GraphParamsMechanicsCache.GridCellSize);
		CustomGridNodeBase nearestNodeXZ2 = vector.GetNearestNodeXZ();
		TransitionCheckerExceptUnit condition = new TransitionCheckerExceptUnit(unit);
		Linecast.LinecastGrid2(nearestNodeXZ.Graph, unit.Position, vector, nearestNodeXZ, out var hit, NNConstraint.None, ref condition);
		GraphNode node = hit.node;
		if (node != nearestNodeXZ && node != null)
		{
			NodeCounter condition2 = default(NodeCounter);
			if (node != nearestNodeXZ2)
			{
				Linecast.LinecastGrid2(nearestNodeXZ.Graph, node.Vector3Position, vector, node, out hit, NNConstraint.None, ref condition2);
			}
			unit.GetOrCreate<UnitPartForceMove>().Push(node, provokeAttackOfOpportunity: false, distanceInCells * 2, caster, condition.StoppedOnUnit);
		}
	}

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
	}

	[Cheat]
	public static void Debug_Force_Move()
	{
		MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
		if (currentUnit == null)
		{
			throw new Exception("Need turn-based mode and current unit");
		}
		Vector3? virtualPosition = Game.Instance.VirtualPositionController.VirtualPosition;
		if (!virtualPosition.HasValue)
		{
			throw new Exception("Need virtual position");
		}
		Vector3 normalized = (virtualPosition.Value - currentUnit.Position).normalized;
		float magnitude = (virtualPosition.Value - currentUnit.Position).magnitude;
		HandleResult(currentUnit, currentUnit, currentUnit.Position - normalized, Mathf.RoundToInt(magnitude / GraphParamsMechanicsCache.GridCellSize));
	}
}
