using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("e732dcf2462749efa78dd209fbfe47e6")]
public class AbilityTargetsBetweenCastPositionAndPrimaryTarget : AbilitySelectTarget, IAbilityAoEPatternProvider
{
	private readonly struct CollectLineNodesCallback : Linecast.ICanTransitionBetweenCells
	{
		private readonly List<CustomGridNodeBase> m_Nodes;

		public CollectLineNodesCallback(List<CustomGridNodeBase> nodes)
		{
			m_Nodes = nodes;
		}

		public bool CanTransitionBetweenCells(CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			m_Nodes.Add(nodeTo);
			return true;
		}
	}

	[SerializeField]
	private bool m_IncludeDead;

	[SerializeField]
	private bool m_IncludeCaster;

	[SerializeField]
	private bool m_ExcludeUnwalkable;

	[SerializeField]
	private bool m_OnlyPrimary;

	public bool ExcludeUnwalkable => m_ExcludeUnwalkable;

	public bool IsIgnoreLos => false;

	public bool UseMeleeLos => false;

	public bool IsIgnoreLevelDifference => false;

	public int PatternAngle => 0;

	public bool CalculateAttackFromPatternCentre => false;

	public TargetType Targets => TargetType.Any;

	public AoEPattern Pattern => null;

	public override IEnumerable<TargetWrapper> Select(AbilityExecutionContext context, TargetWrapper anchor)
	{
		HashSet<TargetWrapper> hashSet = TempHashSet.Get<TargetWrapper>();
		MechanicEntity caster = context.Caster;
		if (m_IncludeCaster)
		{
			hashSet.Add(new TargetWrapper(caster));
		}
		if (m_OnlyPrimary)
		{
			MechanicEntity mechanicEntity = (anchor.HasEntity ? anchor.Entity : null);
			if (mechanicEntity == null)
			{
				CustomGridNodeBase nearestNodeXZUnwalkable = anchor.Point.GetNearestNodeXZUnwalkable();
				if (nearestNodeXZUnwalkable != null && nearestNodeXZUnwalkable.TryGetUnit(out var unit))
				{
					mechanicEntity = unit;
				}
			}
			if (mechanicEntity != null && ShouldTargetEntity(caster, mechanicEntity))
			{
				hashSet.Add(new TargetWrapper(mechanicEntity));
			}
		}
		else
		{
			SelectTargetsBetween(context, caster, anchor, hashSet);
		}
		return hashSet;
	}

	public void OverridePattern(AoEPattern pattern)
	{
	}

	public OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool coveredTargetsOnly = false)
	{
		if (m_OnlyPrimary)
		{
			List<CustomGridNodeBase> list = TempList.Get<CustomGridNodeBase>();
			list.Add(targetNode);
			return new OrientedPatternData(list, casterNode);
		}
		CreateLinePattern(casterNode, targetNode, out var lineNodes);
		return new OrientedPatternData(lineNodes, casterNode);
	}

	private void SelectTargetsBetween(AbilityExecutionContext context, MechanicEntity caster, TargetWrapper anchor, HashSet<TargetWrapper> targets)
	{
		CustomGridNodeBase nearestNodeXZUnwalkable = context.CastPosition.GetNearestNodeXZUnwalkable();
		CustomGridNodeBase bestShootingPosition = context.Ability.GetBestShootingPosition(nearestNodeXZUnwalkable, anchor);
		CustomGridNodeBase nearestNodeXZUnwalkable2 = anchor.Point.GetNearestNodeXZUnwalkable();
		CreateLinePattern(bestShootingPosition, nearestNodeXZUnwalkable2, out var lineNodes);
		foreach (CustomGridNodeBase item in lineNodes)
		{
			if (item.TryGetUnit(out var unit) && ShouldTargetEntity(caster, unit))
			{
				targets.Add(new TargetWrapper(unit));
			}
		}
	}

	private bool CreateLinePattern(CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, out List<CustomGridNodeBase> lineNodes)
	{
		lineNodes = TempList.Get<CustomGridNodeBase>();
		Vector3 vector3Position = casterNode.Vector3Position;
		Vector3 vector3Position2 = targetNode.Vector3Position;
		CollectLineNodesCallback condition = new CollectLineNodesCallback(lineNodes);
		NNConstraint constraint = (m_ExcludeUnwalkable ? NNConstraint.Default : NNConstraint.None);
		Linecast.LinecastGrid2(casterNode.Graph, vector3Position, vector3Position2, casterNode, out var hit, constraint, ref condition);
		return hit.node.position == targetNode.position;
	}

	private bool ShouldTargetEntity(MechanicEntity caster, MechanicEntity entity)
	{
		if (!entity.IsInCombat)
		{
			return false;
		}
		if (!m_IncludeDead && entity != null && entity.IsDeadOrUnconscious)
		{
			return false;
		}
		if ((entity != caster || !m_IncludeCaster) && (bool)entity.Features.IsUntargetable)
		{
			return false;
		}
		return true;
	}
}
