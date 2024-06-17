using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Formations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class InteractionStairsPart : InteractionPart<InteractionStairsSettings>, IHashable
{
	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Action;
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		BaseUnitEntity baseUnitEntity = Game.Instance.SelectionCharacter.SingleSelectedUnit.Value ?? user;
		GraphNode node = ObstacleAnalyzer.GetNearestNode(baseUnitEntity.Position).node;
		GraphNode startNode = base.Settings.NodeLink.StartNode;
		GraphNode endNode = base.Settings.NodeLink.EndNode;
		Vector3 posToMove = ((node == endNode) ? startNode.Vector3Position : ((node != startNode) ? ((Math.Abs(node.Vector3Position.y - endNode.Vector3Position.y) < Math.Abs(node.Vector3Position.y - startNode.Vector3Position.y)) ? ObstacleAnalyzer.FindClosestPointToStandOn(startNode.Vector3Position, user.MovementAgent.Corpulence, (CustomGridNodeBase)startNode) : ObstacleAnalyzer.FindClosestPointToStandOn(endNode.Vector3Position, user.MovementAgent.Corpulence, (CustomGridNodeBase)endNode)) : endNode.Vector3Position));
		MoveOnStairs(baseUnitEntity, posToMove);
	}

	private void MoveOnStairs(BaseUnitEntity selectedUnit, Vector3 posToMove)
	{
		Vector3 defaultDirection = ClickGroundHandler.GetDefaultDirection(posToMove);
		UnitReference[] selectedUnits = Game.Instance.SelectionCharacter.SelectedUnits.Select((BaseUnitEntity u) => u.FromBaseUnitEntity()).ToArray();
		List<BaseUnitEntity> list = Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity c) => c.IsDirectlyControllable).ToList();
		int unitIndex = list.IndexOf(selectedUnit);
		Vector3 worldPosition = PartyFormationHelper.FindFormationCenterFromOneUnit(FormationAnchor.Front, defaultDirection, unitIndex, posToMove, list, selectedUnits);
		UnitCommandsRunner.MoveSelectedUnitsToPointRT(selectedUnit, worldPosition, defaultDirection, isControllerGamepad: true);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
