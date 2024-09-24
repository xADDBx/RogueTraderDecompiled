using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[TypeId("4fe5d760cc3d43949668a3634631c971")]
public class ContextActionMoveDirectToPoint : ContextActionMove
{
	[SerializeField]
	private ContextValue m_Cells;

	[SerializeField]
	private bool m_UseForceMove;

	[SerializeField]
	private bool m_UseJump;

	[SerializeField]
	private bool m_EndInTargetPoint;

	[SerializeField]
	private bool m_ProvokeAttackOfOpportunity;

	[SerializeField]
	private bool m_FromPoint;

	[SerializeField]
	private bool m_IgnoreThreateningArea;

	public override string GetCaption()
	{
		return $"Move direct to {m_TargetPoint}";
	}

	protected override void RunAction()
	{
		CustomGridNodeBase startPoint = base.Caster.Position.GetNearestNodeXZ();
		CustomGridNodeBase nearestNodeXZ = m_TargetPoint.GetValue().GetNearestNodeXZ();
		int num = m_Cells.Calculate(base.Context);
		CustomGridNodeBase endPoint = (m_FromPoint ? (m_TargetPoint.GetValue() + (base.Caster.Position - m_TargetPoint.GetValue()).normalized * ((float)num * GraphParamsMechanicsCache.GridCellSize)).GetNearestNodeXZ() : (base.Caster.Position + (m_TargetPoint.GetValue() - base.Caster.Position).normalized * ((float)num * GraphParamsMechanicsCache.GridCellSize)).GetNearestNodeXZ());
		if (!m_UseJump)
		{
			startPoint = base.TargetEntity.Position.GetNearestNodeXZ();
			nearestNodeXZ = m_TargetPoint.GetValue().GetNearestNodeXZ();
			num = m_Cells.Calculate(base.Context);
			endPoint = (m_FromPoint ? (m_TargetPoint.GetValue() + (base.TargetEntity.Position - m_TargetPoint.GetValue()).normalized * ((float)num * GraphParamsMechanicsCache.GridCellSize)).GetNearestNodeXZ() : (base.TargetEntity.Position + (m_TargetPoint.GetValue() - base.TargetEntity.Position).normalized * ((float)num * GraphParamsMechanicsCache.GridCellSize)).GetNearestNodeXZ());
		}
		if (m_EndInTargetPoint && startPoint.CellDistanceTo(nearestNodeXZ) < num)
		{
			endPoint = nearestNodeXZ;
		}
		if (m_UseJump)
		{
			EventBus.RaiseEvent(delegate(IUnitGetAbilityJump h)
			{
				h.HandleUnitResultJump(startPoint.CellDistanceTo(endPoint), m_TargetPoint.GetValue(), directJump: true, base.Caster, base.Caster, useAttack: false);
			});
			return;
		}
		if (m_UseForceMove)
		{
			EventBus.RaiseEvent(delegate(IUnitGetAbilityPush h)
			{
				h.HandleUnitResultPush(startPoint.CellDistanceTo(endPoint), m_TargetPoint.GetValue(), base.Target.Entity, base.Caster);
			});
			return;
		}
		UnitEntity targetEntity = base.Target?.Entity as UnitEntity;
		if (targetEntity == null || !(targetEntity.View != null) || endPoint == null || !targetEntity.CanMove || !GridAreaHelper.TryGetStandableNode(targetEntity, endPoint, num, out var targetNode))
		{
			return;
		}
		if (base.TargetEntity is AbstractUnitEntity abstractUnitEntity)
		{
			abstractUnitEntity.LookAt(targetNode.Vector3Position);
		}
		targetEntity.Features.CanPassThroughUnits.Retain();
		WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(targetEntity.View.MovementAgent, targetNode.Vector3Position, limitRangeByActionPoints: false, m_IgnoreThreateningArea);
		targetEntity.Features.CanPassThroughUnits.Release();
		warhammerPathPlayer.OverrideBlockMode(targetEntity.View.MovementAgent.Unit ? targetEntity.View.MovementAgent.Unit.BlockMode : warhammerPathPlayer.PathBlockMode);
		int i;
		for (i = 0; i < warhammerPathPlayer.path.Count && warhammerPathPlayer.CanTraverse(warhammerPathPlayer.path[i]); i++)
		{
		}
		using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, this))
		{
			UnitMoveToProperParams unitMoveToProperParams = new UnitMoveToProperParams(ForcedPath.Construct(warhammerPathPlayer.path.GetRange(0, i)), 0f);
			if (m_ProvokeAttackOfOpportunity)
			{
				unitMoveToProperParams.DisableAttackOfOpportunity.Release();
			}
			base.AbilityContext?.TemporarilyBlockLastPathNode(unitMoveToProperParams.ForcedPath, targetEntity);
			targetEntity?.Commands.Run(unitMoveToProperParams);
			EventBus.RaiseEvent(delegate(IUnitAbilityNonPushForceMoveHandler h)
			{
				h.HandleUnitNonPushForceMove(startPoint.CellDistanceTo(endPoint), base.Context, targetEntity);
			});
		}
	}
}
