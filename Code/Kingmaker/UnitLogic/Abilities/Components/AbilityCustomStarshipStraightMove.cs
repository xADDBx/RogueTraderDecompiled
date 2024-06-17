using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("fcda3e2bc7ac9e44c90980b38725dffc")]
public class AbilityCustomStarshipStraightMove : AbilityCustomLogic
{
	[SerializeField]
	private PropertyCalculator straightMoveLength;

	[SerializeField]
	private PropertyCalculator movePointsToSpend;

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		StarshipEntity starship = (StarshipEntity)context.Caster;
		if (starship != null)
		{
			CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)AstarPath.active.GetNearest(starship.Position).node;
			int direction = CustomGraphHelper.GuessDirection(starship.Forward);
			bool startFromOddDiagonal = starship.CombatState.LastDiagonalCount % 2 == 1;
			List<Vector3> list = new List<Vector3>();
			List<GraphNode> list2 = new List<GraphNode>();
			CustomGridNodeBase customGridNodeBase2 = customGridNodeBase;
			int value = straightMoveLength.GetValue(new PropertyContext(starship, null));
			while (customGridNodeBase.CellDistanceTo(customGridNodeBase2) <= value)
			{
				list.Add(customGridNodeBase2.Vector3Position);
				list2.Add(customGridNodeBase2);
				customGridNodeBase2 = customGridNodeBase2.GetNeighbourAlongDirection(direction);
			}
			ForcedPath forcedPath = ForcedPath.Construct(list, list2);
			int num = list.Count;
			while (forcedPath.LengthInCells(startFromOddDiagonal, num) > value)
			{
				num--;
			}
			ForcedPath path = ForcedPath.Construct(forcedPath.vectorPath.Take(num), forcedPath.path.Take(num));
			PathPool.Pool(forcedPath);
			starship.View.StopMoving();
			int num2 = starship.CombatState.LastStraightMoveLength + path.LengthInCells(startFromOddDiagonal);
			int diagonalsCount = starship.CombatState.LastDiagonalCount + path.DiagonalsCount();
			int value2 = movePointsToSpend.GetValue(new PropertyContext(starship, null));
			UnitMoveToProperParams cmd = new UnitMoveToProperParams(path, num2, diagonalsCount, value2);
			UnitCommandHandle moveCmdHandle = starship.Commands.AddToQueueFirst(cmd);
			while (!moveCmdHandle.IsFinished)
			{
				yield return null;
			}
			yield return new AbilityDeliveryTarget(starship);
		}
	}
}
