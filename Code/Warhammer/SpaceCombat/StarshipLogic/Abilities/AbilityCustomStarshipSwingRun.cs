using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Pathfinding;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[TypeId("f0d435adc40f01e408e6f4292f63b498")]
public class AbilityCustomStarshipSwingRun : AbilityCustomLogic, ICustomShipPathProvider, IAbilityTargetRestriction, IAbilityCasterRestriction
{
	private delegate void ProcessNodeDelegate(CustomGridNodeBase currentNode);

	[SerializeField]
	private ActionList StarshipActionsOnFinish;

	[SerializeField]
	private GameObject PassThroughMarker;

	[SerializeField]
	private GameObject FinalNodeMarker;

	[SerializeField]
	private bool IsUpgraded;

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	private int CalcFlyDistance(StarshipEntity starship, Vector3 position)
	{
		PartUnitCombatState combatState = starship.CombatState;
		if (combatState == null)
		{
			return 0;
		}
		PartStarshipNavigation navigation = starship.Navigation;
		if (navigation == null)
		{
			return 0;
		}
		int pushPhaseTilesCount = navigation.PushPhaseTilesCount;
		float num = combatState.ActionPointsBlueMax - (float)pushPhaseTilesCount;
		float num2 = (float)pushPhaseTilesCount - combatState.ActionPointsBlueSpentThisTurn - UnitPredictionManager.Instance.BluePointsCost;
		if (num2 < 0f)
		{
			return 0;
		}
		return (int)(num + num2 / 2f) - 1;
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		StarshipEntity starship = (StarshipEntity)context.Caster;
		if (starship == null)
		{
			yield break;
		}
		CustomGridNodeBase startNode = (CustomGridNodeBase)AstarPath.active.GetNearest(starship.Position).node;
		CustomGridNodeBase[] targetNodes = GetTargetNodes(starship, starship.Position, starship.Forward);
		if (targetNodes == null)
		{
			yield break;
		}
		List<Vector3> list = new List<Vector3>();
		List<GraphNode> list2 = new List<GraphNode>();
		int direction = CustomGraphHelper.GuessDirection(starship.Forward);
		CustomGridNodeBase lastNode = startNode;
		CustomGridNodeBase customGridNodeBase = startNode;
		while (customGridNodeBase != targetNodes[0])
		{
			list.Add(lastNode.Vector3Position);
			list2.Add(lastNode);
			customGridNodeBase = lastNode;
			lastNode = lastNode.GetNeighbourAlongDirection(direction);
		}
		int count = list.Count;
		ForcedPath path = ForcedPath.Construct(list.Take(count), list2.Take(count));
		starship.View.StopMoving();
		bool startFromOddDiagonal = starship.CombatState.LastDiagonalCount % 2 == 1;
		int straightMoveLength = starship.CombatState.LastStraightMoveLength + path.LengthInCells(startFromOddDiagonal);
		int diagonalsCount = starship.CombatState.LastDiagonalCount + path.DiagonalsCount();
		int num = CalcFlyDistance(starship, starship.Position);
		UnitMoveToProperParams cmd = new UnitMoveToProperParams(path, straightMoveLength, diagonalsCount, num + 1);
		UnitCommandHandle moveCmdHandle = starship.Commands.AddToQueueFirst(cmd);
		while (!moveCmdHandle.IsFinished)
		{
			yield return null;
		}
		if (IsUpgraded)
		{
			CustomGridNodeBase customGridNodeBase2 = (CustomGridNodeBase)AstarPath.active.GetNearest(target.Point).node;
			if (customGridNodeBase2 != startNode)
			{
				float y = Quaternion.LookRotation(customGridNodeBase2.Vector3Position - lastNode.Vector3Position).eulerAngles.y;
				starship.SetOrientation(y);
				starship.Position = customGridNodeBase2.Vector3Position;
			}
		}
		else
		{
			starship.SetOrientation((starship.Orientation + 180f) % 360f);
		}
		starship.View.AgentASP.Blocker.BlockAtCurrentPosition();
		using (context.GetDataScope(starship.ToITargetWrapper()))
		{
			StarshipActionsOnFinish.Run();
		}
		yield return new AbilityDeliveryTarget(starship);
	}

	public Dictionary<GraphNode, CustomPathNode> GetCustomPath(StarshipEntity starship, Vector3 position, Vector3 direction)
	{
		Dictionary<GraphNode, CustomPathNode> customPathNodes = new Dictionary<GraphNode, CustomPathNode>();
		_ = (CustomGridNodeBase)AstarPath.active.GetNearest(position).node;
		int directionId = CustomGraphHelper.GuessDirection(direction);
		CustomGridNodeBase parentNode = null;
		CustomGridNodeBase key = GetTargetNodes(starship, position, direction, delegate(CustomGridNodeBase node)
		{
			CustomPathNode value = new CustomPathNode
			{
				Node = node,
				Direction = directionId,
				Parent = ((parentNode != null) ? customPathNodes[parentNode] : null),
				Marker = PassThroughMarker
			};
			customPathNodes.Add(node, value);
			parentNode = node;
		})[0];
		if (customPathNodes.ContainsKey(key))
		{
			CustomPathNode customPathNode = customPathNodes[key];
			int direction2 = CustomGraphHelper.OppositeDirections[directionId];
			customPathNode.Direction = direction2;
			customPathNode.Marker = FinalNodeMarker;
		}
		return customPathNodes;
	}

	private CustomGridNodeBase[] GetTargetNodes(StarshipEntity starship, Vector3 pos, Vector3 dir, ProcessNodeDelegate del = null)
	{
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)AstarPath.active.GetNearest(pos).node;
		CustomGridNodeBase customGridNodeBase2 = null;
		int num = CustomGraphHelper.GuessDirection(dir);
		int num2 = 0;
		int num3 = CalcFlyDistance(starship, pos);
		if (num3 < 0)
		{
			return null;
		}
		int num4 = 0;
		int num5 = num2;
		CustomGridNodeBase customGridNodeBase3 = customGridNodeBase;
		while (true)
		{
			int num6 = 0;
			if (customGridNodeBase2 != null)
			{
				num5 += ((num > 3) ? 1 : 0);
				num6 = ((num5 % 2 != 0 || num <= 3) ? 1 : 2);
			}
			num4 += num6;
			if (num4 > num3)
			{
				break;
			}
			if (num4 > 0)
			{
				del?.Invoke(customGridNodeBase3);
			}
			customGridNodeBase2 = customGridNodeBase3;
			customGridNodeBase3 = customGridNodeBase3.GetNeighbourAlongDirection(num);
		}
		if (IsUpgraded)
		{
			int num7 = CustomGraphHelper.LeftNeighbourDirection[num];
			CustomGridNodeBase customGridNodeBase4 = customGridNodeBase2.GetNeighbourAlongDirection(CustomGraphHelper.LeftNeighbourDirection[num7]);
			if (starship.Navigation.CanStand(customGridNodeBase4, num7))
			{
				del?.Invoke(customGridNodeBase4);
			}
			else
			{
				customGridNodeBase4 = null;
			}
			int num8 = CustomGraphHelper.RightNeighbourDirection[num];
			CustomGridNodeBase customGridNodeBase5 = customGridNodeBase2.GetNeighbourAlongDirection(CustomGraphHelper.RightNeighbourDirection[num8]);
			if (starship.Navigation.CanStand(customGridNodeBase5, num8))
			{
				del?.Invoke(customGridNodeBase5);
			}
			else
			{
				customGridNodeBase5 = null;
			}
			return new CustomGridNodeBase[3] { customGridNodeBase2, customGridNodeBase4, customGridNodeBase5 };
		}
		return new CustomGridNodeBase[1] { customGridNodeBase2 };
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (!(ability.Caster is StarshipEntity starship))
		{
			return false;
		}
		GraphNode node = AstarPath.active.GetNearest(target.Point).node;
		Vector3 currentUnitDirection = UnitPredictionManager.Instance.CurrentUnitDirection;
		return GetTargetNodes(starship, casterPosition, currentUnitDirection)?.Contains(node) ?? false;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return LocalizedTexts.Instance.Reasons.CanOnlyTargetFinalNode;
	}

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		if (!(caster is StarshipEntity starshipEntity))
		{
			return false;
		}
		Vector3 desiredPosition = Game.Instance.VirtualPositionController.GetDesiredPosition(starshipEntity);
		Vector3 currentUnitDirection = UnitPredictionManager.Instance.CurrentUnitDirection;
		CustomGridNodeBase[] targetNodes = GetTargetNodes(starshipEntity, desiredPosition, currentUnitDirection);
		int direction = CustomGraphHelper.OppositeDirections[CustomGraphHelper.GuessDirection(currentUnitDirection)];
		if (targetNodes == null)
		{
			return false;
		}
		return starshipEntity.Navigation.CanStand(targetNodes[0], direction);
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.PathBlocked;
	}
}
