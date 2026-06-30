using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.AI.TargetSelectors;

public class AOETargetSelector : AbilityTargetSelector
{
	private interface INodeVisitor
	{
		bool ShouldSkip(CustomGridNodeBase node);

		void OnNodeEnter(CustomGridNodeBase node);

		bool ShouldMoveNext();
	}

	private readonly struct GatherNodeVisitor : INodeVisitor
	{
		private readonly HashSet<CustomGridNodeBase> m_Nodes;

		public GatherNodeVisitor(HashSet<CustomGridNodeBase> nodes)
		{
			m_Nodes = nodes;
		}

		public bool ShouldSkip(CustomGridNodeBase node)
		{
			return m_Nodes.Contains(node);
		}

		public void OnNodeEnter(CustomGridNodeBase node)
		{
			m_Nodes.Add(node);
		}

		public bool ShouldMoveNext()
		{
			return true;
		}
	}

	private struct HasAnyNodeVisitor : INodeVisitor
	{
		public bool Found;

		public bool ShouldSkip(CustomGridNodeBase node)
		{
			return false;
		}

		public void OnNodeEnter(CustomGridNodeBase node)
		{
			Found = true;
		}

		public bool ShouldMoveNext()
		{
			return !Found;
		}
	}

	private const int MaxNodeToCheckCount = 200;

	public AOETargetSelector(AbilityInfo abilityInfo)
		: base(abilityInfo)
	{
	}

	public override bool HasPossibleTarget(DecisionContext context, CustomGridNodeBase casterNode)
	{
		return HasNodesToCheck(context, casterNode);
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
				if (unit2.CombatGroup.IsEnemy(item2) || unit2.Brain.IsTraitor)
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

	private bool HasNodesToCheck(DecisionContext context, CustomGridNodeBase casterNode)
	{
		if (AbilityInfo.isCharge)
		{
			return GatherChargeAOENodesToCheck(context, casterNode).Count > 0;
		}
		return HasGrenadeAOENodesToCheck(context, casterNode);
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

	private bool HasGrenadeAOENodesToCheck(DecisionContext context, CustomGridNodeBase casterNode)
	{
		HasAnyNodeVisitor visitor = default(HasAnyNodeVisitor);
		ScanGrenadeAOENodes(context, casterNode, ref visitor);
		if (!visitor.Found)
		{
			return AbilityInfo.ability.CanTargetSelf;
		}
		return true;
	}

	private HashSet<CustomGridNodeBase> GatherGrenadeAOENodesToCheck(DecisionContext context, CustomGridNodeBase casterNode)
	{
		HashSet<CustomGridNodeBase> hashSet = new HashSet<CustomGridNodeBase>();
		GatherNodeVisitor visitor = new GatherNodeVisitor(hashSet);
		ScanGrenadeAOENodes(context, casterNode, ref visitor);
		while (hashSet.Count > 200)
		{
			HashSet<CustomGridNodeBase> hashSet2 = new HashSet<CustomGridNodeBase>(hashSet.Count / 2);
			bool flag = true;
			foreach (CustomGridNodeBase item in hashSet)
			{
				if (flag)
				{
					hashSet2.Add(item);
				}
				flag = !flag;
			}
			hashSet = hashSet2;
		}
		if (AbilityInfo.ability.CanTargetSelf)
		{
			hashSet.Add(casterNode);
		}
		return hashSet;
	}

	private void ScanGrenadeAOENodes<TVisitor>(DecisionContext context, CustomGridNodeBase casterNode, ref TVisitor visitor) where TVisitor : struct, INodeVisitor
	{
		IntRect sizeRect = context.Unit.SizeRect;
		CustomGridGraph graph = (CustomGridGraph)casterNode.Graph;
		bool flag = AbilityInfo.ability.TargetAnchor != AbilityTargetAnchor.Point;
		NodeList nodes = GridAreaHelper.GetNodes(casterNode, sizeRect);
		List<TargetInfo> intendedTargets = GetIntendedTargets(context);
		for (int i = 0; i < intendedTargets.Count; i++)
		{
			if (!visitor.ShouldMoveNext())
			{
				break;
			}
			TargetInfo targetInfo = intendedTargets[i];
			if (!IsValidTarget(targetInfo.Entity))
			{
				continue;
			}
			if (flag)
			{
				if (AbilityInfo.ability.CanTargetFromNode(casterNode, targetInfo.Node, new TargetWrapper(targetInfo.Entity), out var _, out var _))
				{
					visitor.OnNodeEnter(targetInfo.Node);
				}
			}
			else if (IsTargetWithinPatternReach(casterNode, sizeRect, targetInfo.Node))
			{
				ScanPatternNodesAroundTarget(context, casterNode, sizeRect, nodes, graph, targetInfo.Node, ref visitor);
			}
		}
	}

	private void ScanPatternNodesAroundTarget<TVisitor>(DecisionContext context, CustomGridNodeBase casterNode, IntRect casterSize, NodeList casterNodes, CustomGridGraph graph, CustomGridNodeBase targetNode, ref TVisitor visitor) where TVisitor : struct, INodeVisitor
	{
		for (int i = AbilityInfo.patternBounds.xmin; i <= AbilityInfo.patternBounds.xmax; i++)
		{
			if (!visitor.ShouldMoveNext())
			{
				break;
			}
			for (int j = AbilityInfo.patternBounds.ymin; j <= AbilityInfo.patternBounds.ymax; j++)
			{
				if (!visitor.ShouldMoveNext())
				{
					break;
				}
				CustomGridNodeBase node = graph.GetNode(targetNode.XCoordinateInGrid - i, targetNode.ZCoordinateInGrid - j);
				if (!visitor.ShouldSkip(node) && IsNodeValid(context, casterNode, casterSize, casterNodes, node, i, j))
				{
					visitor.OnNodeEnter(node);
				}
			}
		}
	}

	private bool IsTargetWithinPatternReach(CustomGridNodeBase casterNode, IntRect casterSize, CustomGridNodeBase targetNode)
	{
		using (ProfileScope.NewScope("IsTargetWithinPatternReach"))
		{
			IntRect patternBounds = AbilityInfo.patternBounds;
			int num = Mathf.Max(patternBounds.xmax, -patternBounds.xmin) + Mathf.Max(patternBounds.ymax, -patternBounds.ymin);
			return WarhammerGeometryUtils.DistanceToInCells(casterNode.Vector3Position, casterSize, targetNode.Vector3Position, default(IntRect)) <= AbilityInfo.maxRange + num;
		}
	}

	private bool IsNodeValid(DecisionContext context, CustomGridNodeBase casterNode, IntRect casterSize, NodeList casterNodes, CustomGridNodeBase node, int offsetX, int offsetY)
	{
		if (node == null || casterNodes.Contains(node) || !node.Walkable)
		{
			return false;
		}
		int num = WarhammerGeometryUtils.DistanceToInCells(casterNode.Vector3Position, casterSize, node.Vector3Position, default(IntRect));
		if (num > AbilityInfo.maxRange || num < AbilityInfo.minRange)
		{
			return false;
		}
		Vector2 normalized = (node.Vector3Position - casterNode.Vector3Position).To2D().normalized;
		(int, int) key = (node.XCoordinateInGrid - casterNode.XCoordinateInGrid, node.ZCoordinateInGrid - casterNode.ZCoordinateInGrid);
		PatternGridData gridData = AbilityInfo.GetOrientedPatternGridDataCached(key, normalized);
		if (!gridData.Contains(new Vector2Int(offsetX, offsetY)))
		{
			return false;
		}
		return CountTargetsInPattern(context, node, in gridData) >= (AbilityInfo.settings?.MustHitTargetsCount ?? 1);
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
