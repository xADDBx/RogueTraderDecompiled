using System;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UnitJumpMoveController : BaseUnitController, IUnitGetAbilityJump, ISubscriber<IBaseUnitEntity>, ISubscriber, IGlobalRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, IGlobalRulebookSubscriber
{
	private void Notify(BaseUnitEntity unit)
	{
		EventBus.RaiseEvent((IBaseUnitEntity)unit, (Action<IUnitGetAbilityJump>)delegate(IUnitGetAbilityJump h)
		{
			h.HandleUnitAbilityJumpDidActed(unit.CombatState.ForceMovedDistanceInCells);
		}, isCheckRuntime: true);
	}

	protected override void TickOnUnit(AbstractUnitEntity entity)
	{
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		UnitPartJump optional = baseUnitEntity.GetOptional<UnitPartJump>();
		if (optional == null)
		{
			return;
		}
		float deltaTime = Game.Instance.TimeController.DeltaTime;
		UnitPartJump.Chunk active = optional.Active;
		if (active == null)
		{
			baseUnitEntity.Position = baseUnitEntity.CurrentNode.position;
			baseUnitEntity.Remove<UnitPartJump>();
			Notify(baseUnitEntity);
			return;
		}
		Vector3 vector = (active.TargetPosition - baseUnitEntity.Position).normalized * Math.Min(active.Speed * deltaTime, Vector3.Distance(active.TargetPosition, baseUnitEntity.Position));
		Vector3 vector2 = baseUnitEntity.Position + vector;
		bool flag = false;
		float num = Vector3.Distance(baseUnitEntity.Position, vector2);
		active.PassedTime += deltaTime;
		if (!(active.PassedTime < active.InClipTime))
		{
			active.PrepareForJump = false;
			baseUnitEntity.ForceLookAt(active.TargetPosition - 2f * (baseUnitEntity.Position - active.TargetPosition));
			baseUnitEntity.Position = vector2;
			if (flag || num == 0f)
			{
				active.PassedTime = active.MaxTime;
			}
			if (active.IsFinished)
			{
				baseUnitEntity.Position = active.TargetPosition;
			}
		}
	}

	public void HandleUnitResultJump(int distanceInCells, Vector3 targetPoint, bool directJump, MechanicEntity target, MechanicEntity caster, bool useAttack)
	{
		HandleResult(caster, target, targetPoint, directJump, -distanceInCells, useAttack);
	}

	public void HandleUnitAbilityJumpDidActed(int distanceInCells)
	{
	}

	private static void HandleResult(MechanicEntity caster, MechanicEntity unit, Vector3 fromPoint, bool directJump, int distanceInCells, bool useAttack)
	{
		CustomGridNodeBase nearestNodeXZ = unit.Position.GetNearestNodeXZ();
		if (nearestNodeXZ == null)
		{
			return;
		}
		CustomGridNodeBase nearestNodeXZ2;
		if (!directJump)
		{
			PartUnitCombatState combatStateOptional = unit.GetCombatStateOptional();
			if (combatStateOptional != null)
			{
				combatStateOptional.ForceMovedDistanceInCells = distanceInCells;
			}
			nearestNodeXZ2 = (unit.Position + (unit.Position - fromPoint).normalized * ((float)distanceInCells * GraphParamsMechanicsCache.GridCellSize)).GetNearestNodeXZ();
		}
		else
		{
			nearestNodeXZ2 = fromPoint.GetNearestNodeXZ();
		}
		if (nearestNodeXZ2 != nearestNodeXZ && nearestNodeXZ2 != null)
		{
			unit.GetOrCreate<UnitPartJump>().Jump(nearestNodeXZ2, provokeAttackOfOpportunity: false, distanceInCells * 2, caster, useAttack);
		}
	}

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
	}
}
