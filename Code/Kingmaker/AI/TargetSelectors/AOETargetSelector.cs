using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.AI.TargetSelectors;

public class AOETargetSelector : AbilityTargetSelector
{
	public AOETargetSelector(AbilityInfo abilityInfo)
		: base(abilityInfo)
	{
	}

	public override bool HasPossibleTarget(DecisionContext context, CustomGridNodeBase casterNode)
	{
		return GatherNodesToCheck(context, casterNode).Count > 0;
	}

	public override TargetWrapper SelectTarget(DecisionContext context, CustomGridNodeBase casterNode)
	{
		Vector3 point = Vector3.zero;
		CustomGridNodeBase node = null;
		HashSet<CustomGridNodeBase> hashSet = GatherNodesToCheck(context, casterNode);
		float num = 0f;
		float hitUnintendedTargetPenalty = context.Unit.Brain.HitUnintendedTargetPenalty;
		List<MechanicEntity> abilityTargets = TempList.Get<MechanicEntity>();
		bool flag = AbilityInfo.ability.TargetAnchor != AbilityTargetAnchor.Point;
		foreach (CustomGridNodeBase item in hashSet)
		{
			if (item == casterNode)
			{
				continue;
			}
			abilityTargets.Clear();
			BaseUnitEntity unit = null;
			if (flag && !item.TryGetUnit(out unit))
			{
				continue;
			}
			TargetWrapper targetWrapper = (flag ? new TargetWrapper(unit) : new TargetWrapper(item.Vector3Position));
			GatherAffectedTargets(casterNode, targetWrapper, in abilityTargets);
			if (abilityTargets.Count == 0)
			{
				continue;
			}
			float num2 = 0f;
			int num3 = 0;
			BaseUnitEntity unit2 = context.Unit;
			Vector3 vector3Position = item.Vector3Position;
			bool flag2 = AbilityInfo.aoeIntendedTargets == TargetType.Ally;
			bool flag3 = !flag2 && !unit2.IsPlayerEnemy;
			foreach (MechanicEntity item2 in abilityTargets)
			{
				if (!IsTargetCounts(item2))
				{
					continue;
				}
				if (unit2.CombatGroup.IsEnemy(item2))
				{
					num2 += ((!flag2) ? (10000f - (item2.Position - vector3Position).sqrMagnitude) : ((0f - hitUnintendedTargetPenalty) * 10000f));
					num3 += ((!flag2) ? 1 : 0);
				}
				else if (unit2.CombatGroup.IsAlly(item2))
				{
					if (flag3 && item2.IsInPlayerParty)
					{
						num2 = float.MinValue;
						break;
					}
					num2 += (flag2 ? (10000f - (item2.Position - vector3Position).sqrMagnitude) : ((0f - hitUnintendedTargetPenalty) * 10000f));
					num3 += (flag2 ? 1 : 0);
				}
				else if (item2 == unit2)
				{
					num2 += (flag2 ? (10000f - (item2.Position - vector3Position).sqrMagnitude) : (-2f * hitUnintendedTargetPenalty * 10000f));
					num3 += (flag2 ? 1 : 0);
				}
			}
			if (num3 >= (AbilityInfo.settings?.MustHitTargetsCount ?? 0) && num2 > num)
			{
				num = num2;
				point = vector3Position;
				node = item;
				base.AffectedTargets.Clear();
				base.AffectedTargets.AddRange(abilityTargets);
			}
		}
		base.SelectedTarget = null;
		if (num > 0f)
		{
			base.SelectedTarget = (flag ? new TargetWrapper(node.GetUnit()) : new TargetWrapper(point));
		}
		return base.SelectedTarget;
	}

	private HashSet<CustomGridNodeBase> GatherNodesToCheck(DecisionContext context, CustomGridNodeBase casterNode)
	{
		if (AbilityInfo.isCharge)
		{
			return GatherChargeAOENodesToCheck(context, casterNode);
		}
		return GatherGrenadeAOENodesToCheck(context, casterNode);
	}

	private HashSet<CustomGridNodeBase> GatherChargeAOENodesToCheck(DecisionContext context, CustomGridNodeBase casterNode)
	{
		HashSet<CustomGridNodeBase> hashSet = new HashSet<CustomGridNodeBase>();
		_ = (CustomGridGraph)casterNode.Graph;
		foreach (TargetInfo intendedTarget in GetIntendedTargets(context))
		{
			if (IsValidTarget(intendedTarget.Entity))
			{
				CustomGridNodeBase node = intendedTarget.Node;
				if (WarhammerGeometryUtils.DistanceToInCells(casterNode.Vector3Position, default(IntRect), node.Vector3Position, default(IntRect)) <= AbilityInfo.maxRange && WarhammerGeometryUtils.DistanceToInCells(casterNode.Vector3Position, default(IntRect), node.Vector3Position, default(IntRect)) >= AbilityInfo.minRange)
				{
					hashSet.Add(node);
				}
			}
		}
		return hashSet;
	}

	private HashSet<CustomGridNodeBase> GatherGrenadeAOENodesToCheck(DecisionContext context, CustomGridNodeBase casterNode)
	{
		HashSet<CustomGridNodeBase> hashSet = new HashSet<CustomGridNodeBase>();
		IntRect sizeRect = context.Unit.SizeRect;
		CustomGridGraph customGridGraph = (CustomGridGraph)casterNode.Graph;
		AoEPattern pattern = AbilityInfo.pattern;
		bool flag = AbilityInfo.ability.TargetAnchor != AbilityTargetAnchor.Point;
		NodeList nodes = GridAreaHelper.GetNodes(casterNode, sizeRect);
		foreach (TargetInfo intendedTarget in GetIntendedTargets(context))
		{
			if (!IsValidTarget(intendedTarget.Entity))
			{
				continue;
			}
			if (flag)
			{
				if (AbilityInfo.ability.CanTargetFromNode(casterNode, intendedTarget.Node, new TargetWrapper(intendedTarget.Entity), out var _, out var _))
				{
					hashSet.Add(intendedTarget.Node);
				}
				continue;
			}
			for (int i = AbilityInfo.patternBounds.xmin; i <= AbilityInfo.patternBounds.xmax; i++)
			{
				for (int j = AbilityInfo.patternBounds.ymin; j <= AbilityInfo.patternBounds.ymax; j++)
				{
					CustomGridNodeBase node = customGridGraph.GetNode(intendedTarget.Node.XCoordinateInGrid - i, intendedTarget.Node.ZCoordinateInGrid - j);
					if (hashSet.Contains(node) || node == null || nodes.Contains(node) || !node.Walkable || WarhammerGeometryUtils.DistanceToInCells(casterNode.Vector3Position, sizeRect, node.Vector3Position, default(IntRect)) > AbilityInfo.maxRange || WarhammerGeometryUtils.DistanceToInCells(casterNode.Vector3Position, sizeRect, node.Vector3Position, default(IntRect)) < AbilityInfo.minRange)
					{
						continue;
					}
					PatternGridData gridData = pattern.GetGridData((node.Vector3Position - casterNode.Vector3Position).To2D().normalized);
					try
					{
						if (gridData.Contains(new Vector2Int(i, j)) && CountTargetsInPattern(context, node, in gridData) >= (AbilityInfo.settings?.MustHitTargetsCount ?? 1))
						{
							hashSet.Add(node);
						}
					}
					finally
					{
						((IDisposable)gridData).Dispose();
					}
				}
			}
		}
		while (hashSet.Count > 200)
		{
			HashSet<CustomGridNodeBase> hashSet2 = new HashSet<CustomGridNodeBase>();
			bool flag2 = true;
			foreach (CustomGridNodeBase item in hashSet)
			{
				if (flag2)
				{
					hashSet2.Add(item);
				}
				flag2 = !flag2;
			}
			hashSet = hashSet2;
		}
		return hashSet;
	}

	private List<TargetInfo> GetIntendedTargets(DecisionContext context)
	{
		if (AbilityInfo.aoeIntendedTargets == TargetType.Ally)
		{
			return context.Allies;
		}
		return context.HatedTargets;
	}

	private void GatherAffectedTargets(CustomGridNodeBase castNode, TargetWrapper targetWrapper, in List<MechanicEntity> abilityTargets)
	{
		if (!AbilityInfo.ability.CanTargetFromNode(castNode, targetWrapper.NearestNode, targetWrapper, out var _, out var _))
		{
			return;
		}
		IAbilityAoEPatternProvider patternProvider = AbilityInfo.patternProvider;
		if (patternProvider == null)
		{
			return;
		}
		foreach (CustomGridNodeBase node in patternProvider.GetOrientedPattern(AbilityInfo, castNode, targetWrapper.NearestNode, coveredTargetsOnly: true).Nodes)
		{
			if (node.TryGetUnit(out var unit))
			{
				abilityTargets.Add(unit);
			}
		}
	}

	private int CountTargetsInPattern(DecisionContext context, CustomGridNodeBase castNode, in PatternGridData gridData)
	{
		using (ProfileScope.NewScope("CountTargetsInPattern"))
		{
			int num = 0;
			foreach (TargetInfo intendedTarget in GetIntendedTargets(context))
			{
				if (IsTargetCounts(intendedTarget.Entity) && gridData.Contains(new Vector2Int(intendedTarget.Node.XCoordinateInGrid - castNode.XCoordinateInGrid, intendedTarget.Node.ZCoordinateInGrid - castNode.ZCoordinateInGrid)))
				{
					num++;
				}
			}
			return num;
		}
	}
}
