using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[TypeId("3dceeebfe2154b33b1ed2de319123fd6")]
public class ContextActionMoveToCoverFromCaster : ContextAction
{
	[SerializeField]
	private int m_movementRange;

	[SerializeField]
	private LosCalculations.CoverType[] m_coverTypesInPreferrableOrder;

	[SerializeField]
	[HideIf("RunFromCaster")]
	private bool MoveToCaster;

	[SerializeField]
	[HideIf("MoveToCaster")]
	private bool RunFromCaster;

	public override string GetCaption()
	{
		string text = "";
		if (MoveToCaster)
		{
			text = "closer ";
		}
		if (RunFromCaster)
		{
			text = "away ";
		}
		return "Move " + text + "to position with required cover from caster";
	}

	protected override void RunAction()
	{
		if (m_coverTypesInPreferrableOrder.Empty() || m_movementRange <= 0 || !(base.Caster is BaseUnitEntity) || !(base.Target?.Entity is BaseUnitEntity baseUnitEntity) || baseUnitEntity.View == null)
		{
			return;
		}
		Dictionary<GraphNode, WarhammerPathPlayerCell> reachableTiles = PathfindingService.Instance.FindAllReachableTiles_Blocking(baseUnitEntity.View.MovementAgent, base.TargetEntity.Position, m_movementRange, ignoreThreateningAreaCost: true);
		if (reachableTiles.Empty())
		{
			return;
		}
		LosCalculations.CoverType coverTypeForNode = GetCoverTypeForNode(baseUnitEntity.CurrentNode.node);
		Dictionary<LosCalculations.CoverType, List<GraphNode>> dictionary = (from node in reachableTiles.Keys
			group node by GetCoverTypeForNode(node)).ToDictionary((IGrouping<LosCalculations.CoverType, GraphNode> group) => group.Key, (IGrouping<LosCalculations.CoverType, GraphNode> group) => group.ToList());
		LosCalculations.CoverType[] coverTypesInPreferrableOrder = m_coverTypesInPreferrableOrder;
		foreach (LosCalculations.CoverType coverType in coverTypesInPreferrableOrder)
		{
			if (coverType == coverTypeForNode && !MoveToCaster && !RunFromCaster)
			{
				break;
			}
			if (!dictionary[coverType].Empty())
			{
				IOrderedEnumerable<GraphNode> source = (MoveToCaster ? dictionary[coverType].OrderBy((GraphNode node) => base.Caster.DistanceTo(reachableTiles[node].Position)) : ((!RunFromCaster) ? dictionary[coverType].OrderBy((GraphNode node) => reachableTiles[node].Length) : dictionary[coverType].OrderByDescending((GraphNode node) => base.Caster.DistanceTo(reachableTiles[node].Position))));
				GraphNode graphNode = source.First();
				WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(baseUnitEntity.View.MovementAgent, graphNode.Vector3Position, limitRangeByActionPoints: false);
				using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, base.Caster))
				{
					base.AbilityContext?.TemporarilyBlockLastPathNode(warhammerPathPlayer, baseUnitEntity);
					UnitMoveToProperParams cmdParams = new UnitMoveToProperParams(ForcedPath.Construct(warhammerPathPlayer), 0f);
					baseUnitEntity.Commands.Run(cmdParams);
					break;
				}
			}
		}
	}

	private LosCalculations.CoverType GetCoverTypeForNode(GraphNode node)
	{
		Vector3 origin = base.Caster.Position;
		BlueprintAbility sourceAbility = base.Context.SourceAbility;
		if (sourceAbility != null && sourceAbility.UseBestShootingPosition)
		{
			origin = LosCalculations.GetBestShootingNode(base.Caster.CurrentUnwalkableNode, base.Caster.SizeRect, base.Target.NearestNode, base.Target.SizeRect).Vector3Position;
		}
		return LosCalculations.GetWarhammerLos(origin, base.Caster.SizeRect, node.Vector3Position, base.Target.SizeRect).CoverType;
	}
}
