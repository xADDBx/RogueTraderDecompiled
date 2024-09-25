using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[TypeId("66f6fdc539a947a0a28892b83dd5729f")]
public class ContextActionMoveToTargetOnShortestPath : ContextAction
{
	[SerializeField]
	[SerializeReference]
	private PositionEvaluator m_TargetPoint;

	[SerializeField]
	private bool m_ConsideringLenght;

	[SerializeField]
	[ShowIf("m_ConsideringLenght")]
	private ContextValue m_Cells;

	[SerializeField]
	private bool m_ProvokeAttackOfOpportunity;

	public override string GetCaption()
	{
		return $"Move unit to the {m_TargetPoint} on the shortest path";
	}

	protected override void RunAction()
	{
		UnitEntity targetEntity = base.Target?.Entity as UnitEntity;
		if (targetEntity == null)
		{
			return;
		}
		Vector3 targetPoint = m_TargetPoint.GetValue();
		Vector3 startingPosition = targetEntity.Position;
		if (!(targetEntity.View != null) || m_TargetPoint == null || !targetEntity.CanMove)
		{
			return;
		}
		int num = m_Cells.Calculate(base.Context);
		if (!GridAreaHelper.TryGetStandableNode(targetEntity, m_TargetPoint.GetValue().GetNearestNodeXZ(), num, out var targetNode))
		{
			return;
		}
		WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(targetEntity.View.MovementAgent, targetNode.Vector3Position, limitRangeByActionPoints: false);
		using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, this))
		{
			if (m_ConsideringLenght && warhammerPathPlayer.vectorPath.Count > num + 1)
			{
				warhammerPathPlayer.vectorPath.RemoveRange(num + 1, warhammerPathPlayer.vectorPath.Count - num - 1);
			}
			base.AbilityContext.TemporarilyBlockLastPathNode(warhammerPathPlayer, targetEntity);
			UnitMoveToProperParams unitMoveToProperParams = new UnitMoveToProperParams(ForcedPath.Construct(warhammerPathPlayer), 0f);
			if (m_ProvokeAttackOfOpportunity)
			{
				unitMoveToProperParams.DisableAttackOfOpportunity.Release();
			}
			targetEntity?.Commands.Run(unitMoveToProperParams);
			EventBus.RaiseEvent(delegate(IUnitAbilityNonPushForceMoveHandler h)
			{
				h.HandleUnitNonPushForceMove(startingPosition.GetNearestNodeXZ().CellDistanceTo(targetPoint.GetNearestNodeXZ()), base.Context, targetEntity);
			});
		}
	}
}
