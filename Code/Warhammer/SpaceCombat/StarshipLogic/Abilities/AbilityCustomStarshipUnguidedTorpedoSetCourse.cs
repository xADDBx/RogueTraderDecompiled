using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.View;
using Pathfinding;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[TypeId("548ecad36993cc4499c5d2534ec91c13")]
public class AbilityCustomStarshipUnguidedTorpedoSetCourse : AbilityCustomLogic, ICustomShipPathProvider, IAbilityCasterRestriction, IAbilityTargetRestriction
{
	private delegate void ProcessNodeDelegate(CustomGridNodeBase currentNode);

	[SerializeField]
	private AoEPattern m_Pattern;

	[SerializeField]
	private GameObject PassThroughMarker;

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		if (context.Caster is StarshipEntity { CombatState: not null } starship)
		{
			List<GraphNode> list = new List<GraphNode>();
			List<Vector3> list2 = new List<Vector3>();
			CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)AstarPath.active.GetNearest(target.Point).node;
			list.Insert(0, customGridNodeBase);
			list2.Insert(0, customGridNodeBase.Vector3Position);
			CustomGridNodeBase customGridNodeBase2 = (CustomGridNodeBase)AstarPath.active.GetNearest(starship.Position).node;
			list.Insert(0, customGridNodeBase2);
			list2.Insert(0, customGridNodeBase2.Vector3Position);
			int count = list2.Count;
			ForcedPath path = ForcedPath.Construct(list2.Take(count), list.Take(count));
			starship.View.StopMoving();
			UnitMovementAgentBase movementAgent = starship.MaybeMovementAgent;
			movementAgent.MaxSpeedOverride = 3f * 30.Feet().Meters / 3f;
			movementAgent.IsCharging = true;
			int warhammerCellDistance = CustomGraphHelper.GetWarhammerCellDistance(customGridNodeBase2, customGridNodeBase);
			int diagonalsCount = ((customGridNodeBase2.XCoordinateInGrid != customGridNodeBase.XCoordinateInGrid && customGridNodeBase2.ZCoordinateInGrid != customGridNodeBase.ZCoordinateInGrid) ? 1 : 0);
			UnitMoveToProperParams cmd = new UnitMoveToProperParams(path, warhammerCellDistance, diagonalsCount, warhammerCellDistance);
			UnitCommandHandle moveCmdHandle = starship.Commands.AddToQueueFirst(cmd);
			while (!moveCmdHandle.IsFinished)
			{
				yield return null;
			}
			movementAgent.MaxSpeedOverride = null;
			movementAgent.IsCharging = false;
			starship.View.AgentASP.Blocker.BlockAtCurrentPosition();
			yield return new AbilityDeliveryTarget(target);
		}
	}

	public Dictionary<GraphNode, CustomPathNode> GetPathData(StarshipEntity starship, Vector3 casterPosition, Vector3 casterDirection)
	{
		Dictionary<GraphNode, CustomPathNode> dictionary = new Dictionary<GraphNode, CustomPathNode>();
		if (casterDirection.sqrMagnitude < float.Epsilon || AlreadyMoved(starship))
		{
			return dictionary;
		}
		int direction = CustomGraphHelper.GuessDirection(casterDirection);
		CustomGridNodeBase applicationNode = (CustomGridNodeBase)AstarPath.active.GetNearest(casterPosition).node;
		foreach (CustomGridNodeBase node in m_Pattern.GetOriented(applicationNode, starship.Forward).Nodes)
		{
			CustomPathNode value = new CustomPathNode
			{
				Node = node,
				Direction = direction,
				Marker = PassThroughMarker
			};
			dictionary.Add(node, value);
		}
		return dictionary;
	}

	private bool AlreadyMoved(StarshipEntity starship)
	{
		return starship.CombatState.ActionPointsBlueSpentThisTurn + UnitPredictionManager.Instance.BluePointsCost > 0f;
	}

	public Dictionary<GraphNode, CustomPathNode> GetCustomPath(StarshipEntity starship, Vector3 position, Vector3 direction)
	{
		return GetPathData(starship, position, direction);
	}

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		if (!(caster is StarshipEntity starship))
		{
			return false;
		}
		return !AlreadyMoved(starship);
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.AlreadyMovedThisTurn;
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (!(ability.Caster is StarshipEntity starship))
		{
			return false;
		}
		CustomGridNodeBase key = (CustomGridNodeBase)AstarPath.active.GetNearest(target.Point).node;
		return GetPathData(starship, casterPosition, UnitPredictionManager.Instance.CurrentUnitDirection).ContainsKey(key);
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return LocalizedTexts.Instance.Reasons.NotAllowedCellToCast;
	}
}
