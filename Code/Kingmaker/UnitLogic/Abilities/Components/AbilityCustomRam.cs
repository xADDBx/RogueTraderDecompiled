using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("644f628e3cd8eee419717e45d4f7a834")]
public class AbilityCustomRam : AbilityCustomLogic, IAbilityTargetRestriction, IAbilityCustomAnimation, IAbilityAoEPatternProvider
{
	[SerializeField]
	private int m_MinRange;

	[SerializeField]
	private int m_MaxRange;

	[SerializeField]
	private bool m_OverrideSpeed;

	[SerializeField]
	[ShowIf("m_OverrideSpeed")]
	private float m_MaxSpeedOverride = 6f;

	[SerializeField]
	private bool m_RamThrough;

	[SerializeField]
	private ActionList m_EndPointActions;

	[SerializeField]
	[ShowIf("m_RamThrough")]
	private ActionList m_RamThroughActions;

	public override bool IsEngageUnit => true;

	public override bool IsMoveUnit => true;

	public bool IsIgnoreLos => false;

	public bool UseMeleeLos => false;

	public bool IsIgnoreLevelDifference => false;

	public int PatternAngle => 0;

	bool IAbilityAoEPatternProvider.CalculateAttackFromPatternCentre => false;

	TargetType IAbilityAoEPatternProvider.Targets => TargetType.Any;

	public AoEPattern Pattern => null;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper targetWrapper)
	{
		if (!(context.Caster is BaseUnitEntity caster))
		{
			PFLog.Default.Error("Caster unit is missing");
			yield break;
		}
		if (caster.GetThreatHandMelee() == null)
		{
			PFLog.Default.Error("Invalid caster's weapon");
			yield break;
		}
		CustomGridNodeBase targetNode = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(targetWrapper.Point).node;
		if (caster.View.AnimationManager?.CurrentAction is UnitAnimationActionHandle unitAnimationActionHandle)
		{
			unitAnimationActionHandle.DoesNotPreventMovement = true;
		}
		else
		{
			PFLog.Default.Error("No animation handle found");
		}
		caster.View.StopMoving();
		caster.MovementAgent.IsCharging = true;
		Vector3 startPosition = caster.Position;
		CustomGridNodeBase startNode = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(startPosition).node;
		OrientedPatternData rayPattern = GetOrientedPattern(context.Ability, startNode, targetNode);
		List<Vector3> list = new List<Vector3>();
		List<GraphNode> list2 = new List<GraphNode>();
		foreach (CustomGridNodeBase node2 in rayPattern.Nodes)
		{
			list.Add(node2.Vector3Position);
			list2.Add(node2);
		}
		ForcedPath p = ForcedPath.Construct(list, list2);
		caster.MovementAgent.ForcePath(p, disableApproachRadius: true);
		float? speedOverride = caster.MovementAgent.MaxSpeedOverride;
		if (m_OverrideSpeed)
		{
			caster.MovementAgent.MaxSpeedOverride = m_MaxSpeedOverride;
		}
		caster.Buffs.Add(BlueprintRoot.Instance.SystemMechanics.ChargeBuff, context, 1.Rounds().Seconds);
		caster.State.IsCharging = true;
		HashSet<BaseUnitEntity> affectedUnits = new HashSet<BaseUnitEntity>();
		while (caster.MovementAgent.IsReallyMoving)
		{
			if (m_RamThrough)
			{
				int passedDistance = caster.DistanceToInCells(startPosition);
				foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
				{
					if (allBaseAwakeUnit == caster || affectedUnits.Contains(allBaseAwakeUnit))
					{
						continue;
					}
					CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)(GraphNode)allBaseAwakeUnit.CurrentNode;
					if (rayPattern.Contains(customGridNodeBase) && customGridNodeBase.CellDistanceTo(startNode) <= passedDistance)
					{
						affectedUnits.Add(allBaseAwakeUnit);
						using (context.GetDataScope(allBaseAwakeUnit.ToITargetWrapper()))
						{
							m_RamThroughActions?.Run();
						}
						yield return new AbilityDeliveryTarget(allBaseAwakeUnit);
					}
				}
			}
			yield return null;
		}
		if (m_OverrideSpeed)
		{
			caster.MovementAgent.MaxSpeedOverride = speedOverride;
		}
		CustomGridNodeBase endNode = (CustomGridNodeBase)(GraphNode)caster.CurrentNode;
		foreach (BaseUnitEntity allBaseAwakeUnit2 in Game.Instance.State.AllBaseAwakeUnits)
		{
			if (allBaseAwakeUnit2 == caster || affectedUnits.Contains(allBaseAwakeUnit2))
			{
				continue;
			}
			CustomGridNodeBase node = (CustomGridNodeBase)(GraphNode)allBaseAwakeUnit2.CurrentNode;
			if (endNode.ContainsConnection(node))
			{
				affectedUnits.Add(allBaseAwakeUnit2);
				using (context.GetDataScope(allBaseAwakeUnit2.ToITargetWrapper()))
				{
					m_EndPointActions?.Run();
				}
				yield return new AbilityDeliveryTarget(allBaseAwakeUnit2);
			}
		}
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
		BaseUnitEntity obj = context.Caster as BaseUnitEntity;
		obj.MovementAgent.IsCharging = false;
		obj.MovementAgent.MaxSpeedOverride = null;
		obj.State.IsCharging = false;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		CheckTargetRestriction(ability.Caster, target, casterPosition, out var failReason);
		return failReason;
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		LocalizedString failReason;
		return CheckTargetRestriction(ability.Caster, target, casterPosition, out failReason);
	}

	private bool CheckTargetRestriction(MechanicEntity caster, TargetWrapper target, Vector3 casterPosition, [CanBeNull] out LocalizedString failReason)
	{
		int num = caster.DistanceToInCells(target.Point);
		if (num < m_MinRange)
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsTooClose;
			return false;
		}
		if (num > m_MaxRange)
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsTooFar;
			return false;
		}
		if (ObstacleAnalyzer.TraceAlongNavmesh(casterPosition, target.Point) != target.Point)
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.ObstacleBetweenCasterAndTarget;
			return false;
		}
		CustomGridNodeBase casterNode = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(casterPosition).node;
		CustomGridNodeBase targetNode = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(target.Point).node;
		int num2 = CalcActualRayLength(caster, casterNode, targetNode);
		if (num2 < m_MinRange)
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsTooClose;
			return false;
		}
		if (num2 < num)
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsTooFar;
			return false;
		}
		failReason = null;
		return true;
	}

	public UnitAnimationActionLink GetAbilityAction(BaseUnitEntity caster)
	{
		return null;
	}

	public void OverridePattern(AoEPattern pattern)
	{
	}

	public OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool coveredTargetsOnly = false)
	{
		int length = CalcActualRayLength(ability.Caster, casterNode, targetNode);
		return GetOrientedRayPattern(ability.Caster, casterNode, targetNode, length);
	}

	private OrientedPatternData GetOrientedRayPattern(MechanicEntity caster, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, int length)
	{
		CustomGridNodeBase innerNodeNearestToTarget = caster.GetInnerNodeNearestToTarget(targetNode.Vector3Position);
		return AoEPattern.Ray(length).GetOriented(casterNode, targetNode.Vector3Position - innerNodeNearestToTarget.Vector3Position);
	}

	private int CalcActualRayLength(MechanicEntity caster, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode)
	{
		OrientedPatternData orientedRay = GetOrientedRayPattern(caster, casterNode, targetNode, m_MaxRange);
		List<CustomGridNodeBase> list = new List<CustomGridNodeBase>();
		foreach (CustomGridNodeBase item in orientedRay.Nodes.OrderBy((CustomGridNodeBase x) => x.CellDistanceTo(orientedRay.ApplicationNode)))
		{
			if (list.Count == 0 || list.Last().ContainsConnection(item))
			{
				list.Add(item);
				continue;
			}
			break;
		}
		int num = casterNode.CellDistanceTo(list.Last());
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
		{
			if (allBaseAwakeUnit == caster || !allBaseAwakeUnit.View || allBaseAwakeUnit.View.MovementAgent.AvoidanceDisabled)
			{
				continue;
			}
			CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)(GraphNode)allBaseAwakeUnit.CurrentNode;
			if (list.Contains(customGridNodeBase))
			{
				int warhammerCellDistance = CustomGraphHelper.GetWarhammerCellDistance(casterNode, customGridNodeBase);
				if (!m_RamThrough && warhammerCellDistance <= num)
				{
					num = warhammerCellDistance - 1;
				}
			}
		}
		if (m_RamThrough && list.Last().ContainsUnit())
		{
			num--;
		}
		return num;
	}
}
