using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[TypeId("bedb007370b34bac85f110b30e5914d0")]
public class ContextActionMoveFromCasterByLine : ContextAction
{
	[SerializeField]
	private ContextValue m_Cells;

	[SerializeField]
	private bool m_UseForceMove;

	[SerializeField]
	private bool m_ProvokeAttackOfOpportunity;

	[SerializeField]
	private bool m_IgnoreThreateningArea;

	public override string GetCaption()
	{
		return $"Move directly from {base.Caster} for {m_Cells.Value} cells";
	}

	protected override void RunAction()
	{
		CustomGridNodeBase startPoint = base.Caster.Position.GetNearestNodeXZ();
		if (startPoint == null)
		{
			return;
		}
		CustomGridNodeBase nearestNodeXZ = base.TargetEntity.Position.GetNearestNodeXZ();
		if (nearestNodeXZ == null)
		{
			return;
		}
		int direction = startPoint.CoordinatesInGrid.Direction(nearestNodeXZ.CoordinatesInGrid);
		int num = m_Cells.Calculate(base.Context);
		CustomGridNodeBase endNode = nearestNodeXZ.GetNeighbourAlongDirection(direction);
		if (endNode == null || !base.TargetEntity.CanStandHere(endNode))
		{
			return;
		}
		for (int i = 1; i < num; i++)
		{
			CustomGridNodeBase neighbourAlongDirection = endNode.GetNeighbourAlongDirection(direction);
			if (neighbourAlongDirection == null || !base.TargetEntity.CanStandHere(neighbourAlongDirection))
			{
				break;
			}
			endNode = neighbourAlongDirection;
		}
		if (m_UseForceMove)
		{
			EventBus.RaiseEvent(delegate(IUnitGetAbilityPush h)
			{
				h.HandleUnitResultPush(startPoint.CellDistanceTo(endNode), endNode.Vector3Position, base.Target.Entity, base.Caster);
			});
			return;
		}
		MechanicEntity mechanicEntity = base.Target?.Entity;
		UnitEntity targetEntity = mechanicEntity as UnitEntity;
		if (targetEntity == null || targetEntity.View == null || !targetEntity.CanMove)
		{
			return;
		}
		if (base.TargetEntity is AbstractUnitEntity abstractUnitEntity)
		{
			abstractUnitEntity.LookAt(endNode.Vector3Position);
		}
		targetEntity.Features.CanPassThroughUnits.Retain();
		WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(targetEntity.View.MovementAgent, endNode.Vector3Position, limitRangeByActionPoints: false, m_IgnoreThreateningArea);
		targetEntity.Features.CanPassThroughUnits.Release();
		warhammerPathPlayer.OverrideBlockMode(targetEntity.View.MovementAgent.Unit ? targetEntity.View.MovementAgent.Unit.BlockMode : warhammerPathPlayer.PathBlockMode);
		int j;
		for (j = 0; j < warhammerPathPlayer.path.Count && warhammerPathPlayer.CanTraverse(warhammerPathPlayer.path[j]); j++)
		{
		}
		using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, this))
		{
			UnitMoveToProperParams unitMoveToProperParams = new UnitMoveToProperParams(ForcedPath.Construct(warhammerPathPlayer.path.GetRange(0, j)), 0f);
			if (m_ProvokeAttackOfOpportunity)
			{
				unitMoveToProperParams.DisableAttackOfOpportunity.Release();
			}
			base.AbilityContext?.TemporarilyBlockLastPathNode(unitMoveToProperParams.ForcedPath, targetEntity);
			targetEntity?.Commands.Run(unitMoveToProperParams);
			EventBus.RaiseEvent(delegate(IUnitAbilityNonPushForceMoveHandler h)
			{
				h.HandleUnitNonPushForceMove(startPoint.CellDistanceTo(endNode), base.Context, targetEntity);
			});
		}
	}
}
