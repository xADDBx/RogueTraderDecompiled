using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Visual.Animation.Kingmaker;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UnitJumpMoveController : BaseUnitController
{
	private static void NotifyFinished(BaseUnitEntity unit)
	{
		EventBus.RaiseEvent((IBaseUnitEntity)unit, (Action<IUnitGetAbilityJump>)delegate(IUnitGetAbilityJump h)
		{
			h.HandleUnitJumpFinished();
		}, isCheckRuntime: true);
	}

	protected override bool ShouldTickOnUnit(AbstractUnitEntity unit)
	{
		if (!unit.IsDisposed)
		{
			return unit.GetOptional<UnitPartJump>() != null;
		}
		return false;
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
		if (active == null || baseUnitEntity.LifeState.IsDead)
		{
			baseUnitEntity.Position = baseUnitEntity.CurrentNode.position;
			baseUnitEntity.Remove<UnitPartJump>();
			NotifyFinished(baseUnitEntity);
			return;
		}
		active.PassedTime += deltaTime;
		if (!(active.PassedTime < active.InClipTime) && (!active.IsMaxFlyTimePassed || active.JumpPhase != UnitPartJump.JumpPhaseType.Out))
		{
			active.JumpPhase = UnitPartJump.JumpPhaseType.Fly;
			baseUnitEntity.ForceLookAt(active.TargetPosition - 2f * (baseUnitEntity.Position - active.TargetPosition));
			Vector3 vector = (active.TargetPosition - baseUnitEntity.Position).normalized * Math.Min(active.Speed * deltaTime, Vector3.Distance(active.TargetPosition, baseUnitEntity.Position));
			Vector3 vector2 = baseUnitEntity.Position + vector;
			float num = Vector3.Distance(baseUnitEntity.Position, vector2);
			baseUnitEntity.Position = vector2;
			if (num == 0f)
			{
				active.PassedTime = active.MaxPassedFlyTime;
			}
			if (active.IsMaxFlyTimePassed)
			{
				active.JumpPhase = UnitPartJump.JumpPhaseType.Out;
				optional.FinishJumpFlyAnimation();
				baseUnitEntity.Position = active.TargetPosition;
			}
		}
	}

	public static bool TryStartJump(MechanicEntity unit, int distanceInCells, Vector3 targetPoint, bool directJump = true, UnitAnimationJumpSubType jumpSubType = UnitAnimationJumpSubType.Jump, float speed = 0f)
	{
		CustomGridNodeBase nearestNodeXZ = unit.Position.GetNearestNodeXZ();
		if (nearestNodeXZ == null)
		{
			return false;
		}
		CustomGridNodeBase customGridNodeBase = (directJump ? targetPoint.GetNearestNodeXZ() : (unit.Position + (unit.Position - targetPoint).normalized * ((float)distanceInCells * GraphParamsMechanicsCache.GridCellSize)).GetNearestNodeXZ());
		if (customGridNodeBase == nearestNodeXZ || customGridNodeBase == null)
		{
			return false;
		}
		return unit.GetOrCreate<UnitPartJump>().Jump(customGridNodeBase, distanceInCells * 2, jumpSubType, speed) != null;
	}
}
