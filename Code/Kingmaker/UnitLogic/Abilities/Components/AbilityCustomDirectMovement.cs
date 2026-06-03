using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("8cbc9755b89b4a81bf497fb24c1144c0")]
public class AbilityCustomDirectMovement : AbilityCustomLogic, IAbilityAoEPatternProvider, IAbilityTargetRestriction
{
	private struct NodeOccupationData
	{
		public bool HasEnemy;

		public bool HasAlly;

		public bool HasUnit;

		public bool HasInvisible;
	}

	public bool StepThroughTarget;

	public bool MustStandInTarget;

	[HideIf("StepThroughTarget")]
	public bool StopOnFirstEncounter;

	[ShowIf("ShowIgnoreFields")]
	public bool IgnoreEnemies;

	[ShowIf("ShowIgnoreFields")]
	public bool IgnoreAllies;

	public bool DamageAllUnitsInLine;

	public bool DisableAttacksOfOpportunity;

	public bool IsCharge;

	[ShowIf("IsCharge")]
	[SerializeField]
	private bool m_OnlyValidIfHitTheTarget;

	public ActionList ActionsOnEncounteredTarget;

	public ActionList ActionsOnCaster;

	[SerializeField]
	private BlueprintBuffReference m_BuffOnMovement;

	[SerializeField]
	private bool m_SkipSpeedOverride;

	[HideIf("m_SkipSpeedOverride")]
	[SerializeField]
	private float m_MaxSpeedOverride = 10f;

	[SerializeField]
	private float m_DelayAfterMovement;

	public BlueprintBuff BuffOnMovement => m_BuffOnMovement.Get();

	public bool IsIgnoreLos => false;

	public bool UseMeleeLos => false;

	public bool IsIgnoreLevelDifference => false;

	public int PatternAngle => 0;

	public bool CalculateAttackFromPatternCentre => false;

	public bool ExcludeUnwalkable => false;

	TargetType IAbilityAoEPatternProvider.Targets => TargetType.Any;

	public AoEPattern Pattern => null;

	public int MinRangeCells => ((BlueprintAbility)base.OwnerBlueprint).MinRange;

	public override bool IsMoveUnit => true;

	public override bool IsEngageUnit => true;

	private bool ShowIgnoreFields
	{
		get
		{
			if (!StepThroughTarget)
			{
				return StopOnFirstEncounter;
			}
			return false;
		}
	}

	private CustomGridNodeController CustomGridNodeController => Game.Instance.CustomGridNodeController;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper clickedTarget)
	{
		MechanicEntity caster = context.Caster;
		if (caster.MaybeMovementAgent == null)
		{
			PFLog.Default.Error("Movement agent is missing");
			yield break;
		}
		CustomGridNodeBase nearestNode = clickedTarget.NearestNode;
		if (!CalculatePathAndTargets(context.Ability, nearestNode, caster.CurrentUnwalkableNode, ignoreInvisibleInCombat: false, out var pathNodes, out var targets))
		{
			PFLog.Ability.ErrorWithReport($"{context.Ability}: can't find path for custom movement");
			yield break;
		}
		Buff buff = caster.Buffs.Add(BuffOnMovement, context, new BuffDuration(null, BuffEndCondition.TurnEndOrCombatEnd));
		try
		{
			int pathCellsCount = pathNodes.Count;
			foreach (AbilityDeliveryTarget item in MoveAlongPath(context, ForcedPath.Construct(pathNodes), targets))
			{
				yield return item;
			}
			if (caster is BaseUnitEntity baseUnitEntity)
			{
				baseUnitEntity.CombatState.RegisterMoveCells(pathCellsCount - 1);
			}
			using (context.GetDataScope(caster.ToITargetWrapper()))
			{
				EventBus.RaiseEvent(delegate(IUnitMovedByAbilityHandler h)
				{
					h.HandleUnitMovedByAbility(context, pathCellsCount);
				});
				ActionsOnCaster.Run();
			}
		}
		finally
		{
			buff?.Remove();
		}
	}

	private bool CalculatePathAndTargets(AbilityData ability, CustomGridNodeBase targetNode, CustomGridNodeBase casterNode, bool ignoreInvisibleInCombat, out List<CustomGridNodeBase> pathNodes, out MechanicEntity[] targets)
	{
		MechanicEntity caster = ability.Caster;
		(OrientedPatternData Pattern, List<CustomGridNodeBase> Path) orientedPatternAndPath = GetOrientedPatternAndPath(ability, casterNode, targetNode, coveredTargetsOnly: false, ignoreInvisibleInCombat);
		OrientedPatternData item = orientedPatternAndPath.Pattern;
		List<CustomGridNodeBase> item2 = orientedPatternAndPath.Path;
		pathNodes = item2;
		int limit = (DamageAllUnitsInLine ? int.MaxValue : caster.Size.GetLesserSide());
		targets = GetAllTargetUnits(item, caster, limit, ignoreInvisibleInCombat);
		return pathNodes.HasItem((CustomGridNodeBase i) => i != casterNode);
	}

	private IEnumerable<AbilityDeliveryTarget> MoveAlongPath(AbilityExecutionContext context, ForcedPath path, MechanicEntity[] targets)
	{
		MechanicEntity caster = context.Caster;
		BaseUnitEntity casterUnit = caster as BaseUnitEntity;
		UnitMovementAgentBase movementAgent = caster.MaybeMovementAgent;
		GraphNode lastNode = path.path.Last();
		if (path.vectorPath.Count == 0)
		{
			yield break;
		}
		float distanceToHandle = Mathf.Sqrt(2f) * 1.Cells().Meters * 1.1f;
		HashSet<MechanicEntity> handledTargets = new HashSet<MechanicEntity>(targets.Length * 2);
		if (!m_SkipSpeedOverride)
		{
			movementAgent.MaxSpeedOverride = ((!m_MaxSpeedOverride.Approximately(0f)) ? m_MaxSpeedOverride : 10f);
		}
		movementAgent.IsCharging = true;
		if (casterUnit != null)
		{
			casterUnit.State.IsCharging = true;
		}
		bool failedToStartPath = false;
		try
		{
			movementAgent.ForcePath(path, disableApproachRadius: true);
		}
		catch (Exception exception)
		{
			failedToStartPath = true;
			PFLog.Ability.ExceptionWithReport(exception, null);
		}
		EventBus.RaiseEvent((IMechanicEntity)caster, (Action<IDirectMovementHandler>)delegate(IDirectMovementHandler h)
		{
			h.HandleDirectMovementStarted(path, DisableAttacksOfOpportunity);
		}, isCheckRuntime: true);
		movementAgent.Blocker.Unblock();
		movementAgent.Blocker.BlockAt(lastNode.Vector3Position);
		TimeSpan startTime = Game.Instance.TimeController.GameTime;
		while (!failedToStartPath && movementAgent.IsReallyMoving)
		{
			yield return null;
			IEnumerable<MechanicEntity> enumerable = HandleNecessaryTargets(context, targets, handledTargets, distanceToHandle);
			foreach (MechanicEntity item in enumerable)
			{
				yield return new AbilityDeliveryTarget(item);
			}
			if (Game.Instance.TimeController.GameTime - startTime > 5f.Seconds())
			{
				PFLog.Default.ErrorWithReport("Direct movement takes too long time, force finished");
				break;
			}
		}
		if (m_DelayAfterMovement > Mathf.Epsilon)
		{
			startTime = Game.Instance.TimeController.GameTime;
			TimeSpan timeSpan = startTime;
			while (timeSpan < startTime + m_DelayAfterMovement.Seconds())
			{
				yield return null;
				timeSpan = Game.Instance.TimeController.GameTime;
			}
		}
		context.Caster.Position = lastNode.Vector3Position;
		movementAgent.IsCharging = false;
		movementAgent.MaxSpeedOverride = null;
		if (casterUnit != null)
		{
			casterUnit.State.IsCharging = false;
		}
		EventBus.RaiseEvent((IMechanicEntity)caster, (Action<IDirectMovementHandler>)delegate(IDirectMovementHandler h)
		{
			h.HandleDirectMovementEnded();
		}, isCheckRuntime: true);
		movementAgent.Blocker.Unblock();
		movementAgent.Blocker.BlockAtCurrentPosition();
		if (StepThroughTarget && casterUnit != null)
		{
			MechanicEntity mechanicEntity = targets.LastItem();
			if (mechanicEntity != null)
			{
				casterUnit.LookAt(SizePathfindingHelper.FromMechanicsToViewPosition(mechanicEntity, mechanicEntity.Position));
			}
		}
		IEnumerable<MechanicEntity> enumerable2 = HandleNecessaryTargets(context, targets, handledTargets, distanceToHandle);
		foreach (MechanicEntity item2 in enumerable2)
		{
			yield return new AbilityDeliveryTarget(item2);
		}
		if (targets.Empty())
		{
			yield return new AbilityDeliveryTarget(context.Caster.Position);
		}
	}

	private IEnumerable<MechanicEntity> HandleNecessaryTargets(AbilityExecutionContext context, MechanicEntity[] targets, HashSet<MechanicEntity> handledTargets, float distanceToHandle)
	{
		if (targets.Empty())
		{
			return Enumerable.Empty<MechanicEntity>();
		}
		List<MechanicEntity> list = TempList.Get<MechanicEntity>();
		MechanicEntity caster = context.Caster;
		foreach (MechanicEntity mechanicEntity in targets)
		{
			if (handledTargets.Contains(mechanicEntity))
			{
				continue;
			}
			CustomGridNodeBase innerNodeNearestToTarget = caster.GetInnerNodeNearestToTarget(mechanicEntity.Position);
			CustomGridNodeBase innerNodeNearestToTarget2 = mechanicEntity.GetInnerNodeNearestToTarget(innerNodeNearestToTarget.Vector3Position);
			if (GeometryUtils.MechanicsDistance(innerNodeNearestToTarget.Vector3Position, innerNodeNearestToTarget2.Vector3Position) <= distanceToHandle)
			{
				try
				{
					HandleTarget(context, mechanicEntity);
				}
				catch (Exception exception)
				{
					PFLog.Default.ExceptionWithReport(exception, null);
				}
				list.Add(mechanicEntity);
				handledTargets.Add(mechanicEntity);
			}
		}
		return list;
	}

	private void HandleTarget(AbilityExecutionContext context, [NotNull] MechanicEntity target)
	{
		using (context.GetDataScope(target.ToITargetWrapper()))
		{
			ActionsOnEncounteredTarget.Run();
		}
	}

	[NotNull]
	[ItemNotNull]
	private MechanicEntity[] GetAllTargetUnits(OrientedPatternData pattern, MechanicEntity caster, int limit, bool ignoreInvisibleInCombat)
	{
		List<CustomGridNodeBase> source = pattern.Nodes.ToTempList();
		List<MechanicEntity> list = TempList.Get<MechanicEntity>();
		using (HashSet<CustomGridNodeBase>.Enumerator enumerator = source.SelectMany((CustomGridNodeBase @base) => caster.GetOccupiedNodes(@base)).ToTempHashSet().GetEnumerator())
		{
			BaseUnitEntity unit;
			while (enumerator.MoveNext() && (!enumerator.Current.TryGetUnit(out unit) || unit == caster || !TryAddTarget(unit, caster, list, ignoreInvisibleInCombat) || --limit >= 1))
			{
			}
		}
		return list.ToArray();
	}

	private bool TryAddTarget(BaseUnitEntity unit, MechanicEntity caster, List<MechanicEntity> targetUnits, bool ignoreInvisibleInCombat)
	{
		if (unit == null || unit.IsDeadOrUnconscious || (bool)unit.Features.IsUntargetable || targetUnits.Contains(unit))
		{
			return false;
		}
		if (ignoreInvisibleInCombat && unit.IsInvisibleInCombat())
		{
			return false;
		}
		if ((IgnoreAllies && caster.IsAlly(unit)) || (IgnoreEnemies && caster.IsEnemy(unit)))
		{
			return false;
		}
		targetUnits.Add(unit);
		return true;
	}

	public IComparer<CustomGridNodeBase> DistanceComparer(MechanicEntity caster)
	{
		return Comparer<CustomGridNodeBase>.Create((CustomGridNodeBase a, CustomGridNodeBase b) => Comparer<float>.Default.Compare(caster.DistanceToInCells(a.Vector3Position), caster.DistanceToInCells(b.Vector3Position)));
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
		if (context.Caster is UnitEntity unitEntity)
		{
			unitEntity.View.MovementAgent.IsCharging = false;
			unitEntity.View.MovementAgent.MaxSpeedOverride = null;
			unitEntity.State.IsCharging = false;
		}
	}

	public void OverridePattern(AoEPattern pattern)
	{
	}

	public OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool coveredTargetsOnly = false)
	{
		return GetOrientedPatternAndPath(ability, casterNode, targetNode, coveredTargetsOnly, ignoreInvisibleInCombat: true).Pattern;
	}

	public (OrientedPatternData Pattern, List<CustomGridNodeBase> Path) GetOrientedPatternAndPath(IAbilityDataProviderForPattern ability, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool coveredTargetsOnly = false, bool ignoreInvisibleInCombat = false)
	{
		MechanicEntity caster = ability.Caster;
		int rangeCells = ability.RangeCells;
		List<CustomGridNodeBase> list = GetPath(ability.Data, casterNode, targetNode);
		if (list.Empty() || (!StepThroughTarget && !IsPathToTargetReachDestination(caster, targetNode, list)))
		{
			List<CustomGridNodeBase> list2 = TempList.Get<CustomGridNodeBase>();
			list2.Add(targetNode);
			return (Pattern: new OrientedPatternData(list2, casterNode), Path: TempList.Get<CustomGridNodeBase>());
		}
		CustomGridNodeBase customGridNodeBase = list[0];
		Vector2Int vector2Int = casterNode.CoordinatesInGrid - customGridNodeBase.CoordinatesInGrid;
		List<Vector2Int> list3 = (from i in caster.GetSortedNodesByDistanceToTarget(casterNode, targetNode.Vector3Position)
			select i.CoordinatesInGrid - casterNode.CoordinatesInGrid).ToTempList();
		CustomGridGraph g2 = (CustomGridGraph)customGridNodeBase.Graph;
		List<CustomGridNodeBase> list4 = TempList.Get<CustomGridNodeBase>();
		HashSet<CustomGridNodeBase> hashSet = TempHashSet.Get<CustomGridNodeBase>();
		for (int j = 0; j < list.Count; j++)
		{
			CustomGridNodeBase customGridNodeBase2 = list[j];
			CustomGridNodeBase customGridNodeBase3 = GetNode(g2, customGridNodeBase2.CoordinatesInGrid + vector2Int);
			if (customGridNodeBase3 == null)
			{
				break;
			}
			int warhammerLength = CustomGraphHelper.GetWarhammerLength(customGridNodeBase3.CoordinatesInGrid - casterNode.CoordinatesInGrid);
			if (!StepThroughTarget && warhammerLength > rangeCells)
			{
				break;
			}
			hashSet.Clear();
			foreach (Vector2Int item in list3)
			{
				hashSet.Add(GetNode(g2, customGridNodeBase3.CoordinatesInGrid + item));
			}
			NodeOccupationData nodeOccupationData = GetNodeOccupationData(caster, hashSet);
			if (!coveredTargetsOnly || nodeOccupationData.HasUnit)
			{
				list4.AddRange(hashSet);
			}
			if (!StepThroughTarget && StopOnFirstEncounter && ((!IgnoreAllies && nodeOccupationData.HasAlly) || (!IgnoreEnemies && nodeOccupationData.HasEnemy)))
			{
				if (!nodeOccupationData.HasInvisible)
				{
					break;
				}
				if (!ignoreInvisibleInCombat)
				{
					list = list.Take(j).ToTempList();
					break;
				}
			}
		}
		if (IsCharge && !list4.Contains(targetNode))
		{
			list4.Add(targetNode);
		}
		return (Pattern: new OrientedPatternData(list4, list4.FirstItem()), Path: list);
		static CustomGridNodeBase GetNode(NavGraph g, Vector2Int i)
		{
			return ((CustomGridGraph)g).GetNode(i.x, i.y);
		}
	}

	private NodeOccupationData GetNodeOccupationData(MechanicEntity caster, HashSet<CustomGridNodeBase> nodesToCheck)
	{
		NodeOccupationData result = default(NodeOccupationData);
		foreach (CustomGridNodeBase item in nodesToCheck)
		{
			BaseUnitEntity unit = item.GetUnit();
			if (unit != null && unit != caster && unit.IsConscious)
			{
				result.HasUnit = true;
				result.HasEnemy |= unit.IsEnemy(caster);
				result.HasAlly |= unit.IsAlly(caster);
				result.HasInvisible |= unit.IsInvisibleInCombat();
			}
		}
		return result;
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper targetWrapper, Vector3 casterPosition)
	{
		LocalizedString failReason;
		return CheckTargetRestriction(ability, targetWrapper, casterPosition, out failReason);
	}

	[CanBeNull]
	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper targetWrapper, Vector3 casterPosition)
	{
		CheckTargetRestriction(ability, targetWrapper, casterPosition, out var failReason);
		return failReason?.Text;
	}

	private bool CheckTargetRestriction(AbilityData ability, TargetWrapper target, Vector3 casterPosition, [CanBeNull] out LocalizedString failReason)
	{
		CustomGridNodeBase nearestNode = target.NearestNode;
		CustomGridNodeBase nearestNodeXZUnwalkable = casterPosition.GetNearestNodeXZUnwalkable();
		if (!CalculatePathAndTargets(ability, target.NearestNode, nearestNodeXZUnwalkable, ignoreInvisibleInCombat: true, out var pathNodes, out var targets))
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.CanNotReachTarget;
			return false;
		}
		if (StopOnFirstEncounter && IsCharge && !targets.Empty() && !targets[^1].GetOccupiedNodes().Contains(nearestNode))
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.CanNotReachTarget;
			return false;
		}
		if (MustStandInTarget)
		{
			if (!pathNodes.Empty())
			{
				List<CustomGridNodeBase> list = pathNodes;
				if (list[list.Count - 1] == nearestNode)
				{
					MechanicEntity caster = ability.Caster;
					List<CustomGridNodeBase> list2 = pathNodes;
					if (caster.CanStandHere(list2[list2.Count - 1]))
					{
						goto IL_00e4;
					}
				}
			}
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsInvalid;
			return false;
		}
		goto IL_00e4;
		IL_00e4:
		if (!StepThroughTarget && !IsPathToTargetReachDestination(ability.Caster, nearestNode, pathNodes))
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsTooFar;
			return false;
		}
		CustomGridNodeBase customGridNodeBase = pathNodes.LastItem();
		if (customGridNodeBase == null || nearestNodeXZUnwalkable.CellDistanceTo(customGridNodeBase) < MinRangeCells)
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsTooClose;
			return false;
		}
		if (IsCharge && WarhammerGeometryUtils.DistanceToInCells(nearestNodeXZUnwalkable.Vector3Position, ability.Caster.SizeRect, target.Entity?.GetInnerNodeNearestToTarget(target.Point).Vector3Position ?? target.NearestNode.Vector3Position, default(IntRect)) > ability.RangeCells)
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsTooFar;
			return false;
		}
		failReason = null;
		return true;
	}

	private List<CustomGridNodeBase> GetPath(AbilityData abilityData, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode)
	{
		if (!StepThroughTarget)
		{
			return GetPathToTarget(abilityData, casterNode, targetNode);
		}
		return GetStepThroughTargetPath(abilityData.Caster, casterNode, targetNode);
	}

	private List<CustomGridNodeBase> GetPathToTarget(AbilityData abilityData, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode)
	{
		BaseUnitEntity baseUnitEntity = targetNode.GetUnit();
		if (baseUnitEntity != null && baseUnitEntity.IsDeadOrUnconscious && abilityData.CanTargetPoint)
		{
			baseUnitEntity = null;
		}
		return PathfindingService.Instance.FindPathChargeTB_Blocking(abilityData.Caster.MaybeMovementAgent, casterNode.Vector3Position, targetNode.Vector3Position, !StopOnFirstEncounter || IgnoreEnemies || IgnoreAllies, baseUnitEntity).path.Cast<CustomGridNodeBase>().ToTempList();
	}

	private bool IsPathToTargetReachDestination(MechanicEntity caster, CustomGridNodeBase targetNode, List<CustomGridNodeBase> path)
	{
		if (path.Empty())
		{
			return false;
		}
		return caster.GetOccupiedNodes(path[path.Count - 1].Vector3Position).Any((CustomGridNodeBase i) => DistanceToInCells(i, caster.SizeRect, targetNode) < 2 && i.HasMeleeLos(targetNode));
		static int DistanceToInCells(CustomGridNodeBase origin, IntRect size, CustomGridNodeBase target)
		{
			return WarhammerGeometryUtils.DistanceToInCells(origin.Vector3Position, size, target.Vector3Position, default(IntRect));
		}
	}

	private List<CustomGridNodeBase> GetStepThroughTargetPath(MechanicEntity caster, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode)
	{
		List<CustomGridNodeBase> result = TempList.Get<CustomGridNodeBase>();
		BaseUnitEntity unit = targetNode.GetUnit();
		if (unit == null)
		{
			return result;
		}
		CustomGridNodeBase innerNodeNearestToTarget = caster.GetInnerNodeNearestToTarget(casterNode, targetNode.Vector3Position);
		Vector2 normalized = (unit.GetInnerNodeNearestToTarget(innerNodeNearestToTarget.Vector3Position).Vector3Position - innerNodeNearestToTarget.Vector3Position).To2D().normalized;
		if (normalized.sqrMagnitude < 1E-06f)
		{
			return result;
		}
		Linecast.Ray2NodeOffsets offsets = new Linecast.Ray2NodeOffsets(casterNode.CoordinatesInGrid, normalized);
		foreach (CustomGridNodeBase item in new Linecast.Ray2Nodes((CustomGridGraph)casterNode.Graph, in offsets))
		{
			if (result.Any())
			{
				List<CustomGridNodeBase> list = result;
				if (!CustomGraphHelper.HasConnectionBetweenNodes(list[list.Count - 1], item))
				{
					ComeBack(2, out var comeBackNode2);
					if (comeBackNode2 != casterNode)
					{
						BaseUnitEntity unit2 = comeBackNode2.GetUnit();
						if (unit2 != null && unit2 != caster && !unit2.IsDeadOrUnconscious && caster.IsEnemy(unit2))
						{
							ComeBack(4, out var _);
						}
					}
					return result;
				}
			}
			result.Add(item);
			bool flag = false;
			bool flag2 = false;
			foreach (CustomGridNodeBase occupiedNode in caster.GetOccupiedNodes(item.Vector3Position))
			{
				BaseUnitEntity unit3 = occupiedNode.GetUnit();
				flag2 = flag2 || unit3 != null;
				if (unit3 != null && unit3 != caster && unit3 != unit)
				{
					result.Clear();
					flag = true;
					break;
				}
			}
			if (flag || !flag2)
			{
				break;
			}
		}
		return result;
		void ComeBack(int previousNodeIndex, out CustomGridNodeBase comeBackNode)
		{
			CustomGridNodeBase customGridNodeBase;
			if (result.Count < previousNodeIndex)
			{
				customGridNodeBase = casterNode;
			}
			else
			{
				List<CustomGridNodeBase> list2 = result;
				customGridNodeBase = list2[list2.Count - previousNodeIndex];
			}
			comeBackNode = customGridNodeBase;
			result.Add(comeBackNode);
		}
	}
}
