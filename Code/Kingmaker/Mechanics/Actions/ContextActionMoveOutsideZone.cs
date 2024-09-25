using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[TypeId("8cfda13e37ea4234a6015ed326d9ceb3")]
public class ContextActionMoveOutsideZone : ContextAction
{
	[SerializeField]
	[SerializeReference]
	private PositionEvaluator m_TargetPoint;

	[SerializeField]
	private AoEPattern m_Zone;

	[SerializeField]
	private bool m_UseForceMove;

	[SerializeField]
	private bool m_ProvokeAttackOfOpportunity;

	public override string GetCaption()
	{
		return $"Move unit out of {m_Zone} in {m_TargetPoint}";
	}

	protected override void RunAction()
	{
		CustomGridNodeBase startPoint = base.TargetEntity.Position.GetNearestNodeXZ();
		CustomGridNodeBase nearestNodeXZ = m_TargetPoint.GetValue().GetNearestNodeXZ();
		int num = int.MaxValue;
		CustomGridNodeBase randomCell = null;
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)AstarPath.active.GetNearest(base.TargetEntity.Position).node;
		if (base.TargetEntity.MaybeMovementAgent != null && base.TargetEntity is BaseUnitEntity baseUnitEntity)
		{
			Dictionary<GraphNode, WarhammerPathPlayerCell> dictionary = PathfindingService.Instance.FindAllReachableTiles_Blocking(base.TargetEntity.MaybeMovementAgent, base.TargetEntity.Position, 10f, ignoreThreateningAreaCost: true);
			if (dictionary.Count == 0)
			{
				return;
			}
			IOrderedEnumerable<KeyValuePair<GraphNode, WarhammerPathPlayerCell>> orderedEnumerable = dictionary.OrderBy((KeyValuePair<GraphNode, WarhammerPathPlayerCell> x) => x.Value.Length);
			List<GraphNode> list = new List<GraphNode>();
			foreach (var (graphNode2, warhammerPathPlayerCell2) in orderedEnumerable)
			{
				if (graphNode2 != null && graphNode2 != customGridNodeBase && warhammerPathPlayerCell2.IsCanStand && (!(graphNode2 is CustomGridNodeBase node) || nearestNodeXZ == null || !m_Zone.GetOriented(base.Context.MainTarget.NearestNode, nearestNodeXZ.Vector3Position - base.TargetEntity.Position).Contains(node)))
				{
					if ((int)warhammerPathPlayerCell2.Length > num)
					{
						break;
					}
					num = (int)warhammerPathPlayerCell2.Length;
					list.Add(graphNode2);
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			randomCell = (CustomGridNodeBase)list[baseUnitEntity.Random.Range(0, list.Count)];
		}
		if (m_UseForceMove)
		{
			EventBus.RaiseEvent(delegate(IUnitGetAbilityPush h)
			{
				h.HandleUnitResultPush(startPoint.CellDistanceTo(randomCell), m_TargetPoint.GetValue(), base.Target.Entity, base.Caster);
			});
			return;
		}
		UnitEntity targetEntity = base.Target?.Entity as UnitEntity;
		if (targetEntity == null || !(targetEntity.View != null) || randomCell == null || !targetEntity.CanMove)
		{
			return;
		}
		WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(targetEntity.View.MovementAgent, randomCell.Vector3Position, limitRangeByActionPoints: false);
		using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, this))
		{
			UnitMoveToProperParams unitMoveToProperParams = new UnitMoveToProperParams(ForcedPath.Construct(warhammerPathPlayer), 0f);
			if (m_ProvokeAttackOfOpportunity)
			{
				unitMoveToProperParams.DisableAttackOfOpportunity.Release();
			}
			base.AbilityContext?.TemporarilyBlockLastPathNode(warhammerPathPlayer, targetEntity);
			targetEntity?.Commands.Run(unitMoveToProperParams);
			EventBus.RaiseEvent(delegate(IUnitAbilityNonPushForceMoveHandler h)
			{
				h.HandleUnitNonPushForceMove(startPoint.CellDistanceTo(randomCell), base.Context, targetEntity);
			});
		}
	}
}
