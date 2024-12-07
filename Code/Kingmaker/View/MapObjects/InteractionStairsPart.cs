using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UI.Common;
using Kingmaker.View.MapObjects.InteractionComponentBase;
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
		BaseUnitEntity value = Game.Instance.SelectionCharacter.SelectedUnit.Value;
		if (value != null)
		{
			user = value;
		}
		if (user.IsDirectlyControllable())
		{
			GraphNode node = ObstacleAnalyzer.GetNearestNode(user.Position).node;
			GraphNode startNode = base.Settings.NodeLink.StartNode;
			GraphNode endNode = base.Settings.NodeLink.EndNode;
			Vector3 posToMove = ((node == endNode) ? startNode.Vector3Position : ((node != startNode) ? ((Math.Abs(node.Vector3Position.y - endNode.Vector3Position.y) < Math.Abs(node.Vector3Position.y - startNode.Vector3Position.y)) ? ObstacleAnalyzer.FindClosestPointToStandOn(startNode.Vector3Position, user.MovementAgent.Corpulence, (CustomGridNodeBase)startNode) : ObstacleAnalyzer.FindClosestPointToStandOn(endNode.Vector3Position, user.MovementAgent.Corpulence, (CustomGridNodeBase)endNode)) : endNode.Vector3Position));
			MoveOnStairs(user, posToMove);
		}
	}

	private static void MoveOnStairs(BaseUnitEntity selectedUnit, Vector3 posToMove)
	{
		Vector3 defaultDirection = ClickGroundHandler.GetDefaultDirection(posToMove);
		List<BaseUnitEntity> selectedUnits = Game.Instance.SelectionCharacter.SelectedUnits.Where((BaseUnitEntity u) => u.IsDirectlyControllable() && !u.MovementAgent.IsTraverseInProgress).ToList();
		List<BaseUnitEntity> allUnits = Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity c) => c.IsDirectlyControllable()).ToList();
		UnitCommandsRunner.MoveSelectedUnitsToPointRT(selectedUnit, posToMove, defaultDirection, isControllerGamepad: false, anchorOnMainUnit: true, preview: false, 1f, selectedUnits, null, allUnits);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
