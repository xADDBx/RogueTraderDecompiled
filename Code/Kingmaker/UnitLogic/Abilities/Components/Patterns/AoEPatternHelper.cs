using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Patterns;

public static class AoEPatternHelper
{
	private readonly struct NodeCollector : Linecast.ICanTransitionBetweenCells
	{
		private readonly List<CustomGridNodeBase> m_NodesOnLine;

		public NodeCollector(List<CustomGridNodeBase> nodesOnLine)
		{
			m_NodesOnLine = nodesOnLine;
		}

		public bool CanTransitionBetweenCells(CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			m_NodesOnLine.Add(nodeTo);
			return true;
		}
	}

	public static CustomGridNodeBase GetActualCastNode([NotNull] MechanicEntity caster, Vector3 castPosition, Vector3 target, int minRange = 0, int maxRange = int.MaxValue)
	{
		return GetActualCastNode(caster, castPosition.GetNearestNodeXZUnwalkable(), target, minRange, maxRange);
	}

	public static CustomGridNodeBase GetActualCastNode([NotNull] MechanicEntity caster, CustomGridNodeBase castNode, CustomGridNodeBase targetNode, int minRange = 0, int maxRange = int.MaxValue)
	{
		return GetActualCastNode(caster, castNode, targetNode.Vector3Position, minRange, maxRange);
	}

	public static Vector3 GetActualCastPosition([NotNull] MechanicEntity caster, Vector3 castPosition, Vector3 target, int minRange, int maxRange)
	{
		return GetActualCastNode(caster, castPosition, target, minRange, maxRange).Vector3Position;
	}

	public static Vector3 GetActualCastPosition(MechanicEntity caster, CustomGridNodeBase casterPosition, Vector3 target, int minRange, int maxRange)
	{
		return GetActualCastNode(caster, casterPosition, target, minRange, maxRange).Vector3Position;
	}

	public static CustomGridNodeBase GetActualCastNode([NotNull] MechanicEntity casterEntity, CustomGridNodeBase castNode, Vector3 target, int minRange, int maxRange)
	{
		CustomGridNodeBase innerNodeNearestToTarget = casterEntity.GetInnerNodeNearestToTarget(castNode, target);
		NavGraph graph = castNode.Graph;
		List<CustomGridNodeBase> list = TempList.Get<CustomGridNodeBase>();
		list.Add(innerNodeNearestToTarget);
		NodeCollector condition = new NodeCollector(list);
		Linecast.LinecastGrid(graph, innerNodeNearestToTarget.Vector3Position, target, innerNodeNearestToTarget, out var hit, ref condition);
		int i = list.Count - 1;
		while (i >= 0 && innerNodeNearestToTarget.CellDistanceTo(list[i]) > maxRange)
		{
			i--;
		}
		int num = innerNodeNearestToTarget.CellDistanceTo(list[i]);
		if (num < minRange)
		{
			list.Clear();
			Vector3 vector3Position = innerNodeNearestToTarget.Vector3Position;
			if ((double)(target - vector3Position).sqrMagnitude < 0.0001)
			{
				target += Vector3.back;
			}
			target = vector3Position + (target - vector3Position).normalized * (minRange * 2);
			Linecast.LinecastGrid(graph, vector3Position, target, castNode, out hit, ref condition);
			for (i = 0; i < list.Count - 1 && innerNodeNearestToTarget.CellDistanceTo(list[i]) < minRange; i++)
			{
			}
		}
		bool num2 = list.Empty();
		num = ((!num2) ? innerNodeNearestToTarget.CellDistanceTo(list[i]) : 0);
		if (num2 || num < minRange || num > maxRange)
		{
			return castNode;
		}
		return list[i];
	}

	public static bool WouldTargetEntityPattern(MechanicEntity entity, OrientedPatternData pattern, Vector3 castPosition, float distance, out LosDescription los)
	{
		if ((entity.Position - castPosition).sqrMagnitude > distance * distance)
		{
			los = new LosDescription(LosCalculations.CoverType.None);
			return false;
		}
		los = LosCalculations.GetWarhammerLos(castPosition, default(IntRect), entity);
		return WouldTargetEntity(pattern, entity);
	}

	public static bool WouldTargetEntity(OrientedPatternData coveredNodes, MechanicEntity entity)
	{
		foreach (CustomGridNodeBase occupiedNode in entity.GetOccupiedNodes())
		{
			if (coveredNodes.Contains(occupiedNode))
			{
				return true;
			}
		}
		return false;
	}

	public static CustomGridNodeBase GetGridNode(Vector3 pos)
	{
		return (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(pos).node;
	}

	public static Vector3 GetGridAdjustedPosition(Vector3 pos)
	{
		GraphNode node = ObstacleAnalyzer.GetNearestNode(pos).node;
		if (node != null)
		{
			return (Vector3)node.position;
		}
		return pos;
	}

	public static OrientedPatternData GetOrientedPattern([CanBeNull] IAbilityDataProviderForPattern ability, [NotNull] MechanicEntity caster, AoEPattern pattern, IAbilityAoEPatternProvider provider, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool castOnSameLevel, bool directional, bool coveredTargetsOnly, out CustomGridNodeBase actualCastNode)
	{
		actualCastNode = GetActualCastNode(caster, casterNode, targetNode);
		CustomGridNodeBase innerNodeNearestToTarget = caster.GetInnerNodeNearestToTarget(casterNode, targetNode.Vector3Position);
		CustomGridNodeBase outerNodeNearestToTarget = caster.GetOuterNodeNearestToTarget(casterNode, targetNode.Vector3Position);
		Vector3 vector3Position = actualCastNode.Vector3Position;
		if (castOnSameLevel && Mathf.Abs(innerNodeNearestToTarget.Vector3Position.y - vector3Position.y) > 1.6f)
		{
			return OrientedPatternData.Empty;
		}
		using (ProfileScope.New("GetOriented"))
		{
			CustomGridNodeBase checkLosFromNode = (provider.CalculateAttackFromPatternCentre ? actualCastNode : innerNodeNearestToTarget);
			CustomGridNodeBase customGridNodeBase = ((directional || (ability != null && ability.IsScatter)) ? outerNodeNearestToTarget : actualCastNode);
			Vector3 castDirection = AoEPattern.GetCastDirection(pattern.Type, innerNodeNearestToTarget, customGridNodeBase, targetNode);
			AbilityData abilityData = ability?.Data;
			MechanicEntity mechanicEntity = caster;
			if (ContextData<MechanicsContext.Data>.Current?.Context is AbilityExecutionContext abilityExecutionContext)
			{
				abilityData = abilityExecutionContext.Ability;
				mechanicEntity = abilityExecutionContext.MaybeCaster ?? mechanicEntity;
			}
			castDirection = ((mechanicEntity is AreaEffectEntity entity) ? entity.ApplyFlipPattern(castDirection) : mechanicEntity.ApplyFlipPattern(abilityData, castDirection));
			return pattern.GetOriented(checkLosFromNode, customGridNodeBase, castDirection, provider.IsIgnoreLos, provider.IsIgnoreLevelDifference, directional, coveredTargetsOnly, provider.UseMeleeLos);
		}
	}
}
