using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
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

	public BlueprintBuff BuffOnMovement => m_BuffOnMovement.Get();

	public bool IsIgnoreLos => false;

	public bool UseMeleeLos => false;

	public bool IsIgnoreLevelDifference => false;

	public int PatternAngle => 0;

	public bool CalculateAttackFromPatternCentre => false;

	TargetType IAbilityAoEPatternProvider.Targets => TargetType.Any;

	public AoEPattern Pattern => null;

	public int MinRangeCells => ((BlueprintAbility)base.OwnerBlueprint).MinRange;

	public bool CanTargetPoint => ((BlueprintAbility)base.OwnerBlueprint).CanTargetPoint;

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

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper clickedTarget)
	{
		MechanicEntity caster = context.Caster;
		if (caster.MaybeMovementAgent == null)
		{
			PFLog.Default.Error("Movement agent is missing");
			yield break;
		}
		CustomGridNodeBase nearestNode = clickedTarget.NearestNode;
		if (!CalculatePathAndTargets(context.Ability, nearestNode, caster.CurrentUnwalkableNode, out var pathNodes, out var targets))
		{
			PFLog.Ability.ErrorWithReport($"{context.Ability}: can't find path for custom movement");
			yield break;
		}
		Buff buff = caster.Buffs.Add(BuffOnMovement, context, null);
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
				ActionsOnCaster.Run();
			}
		}
		finally
		{
			buff?.Remove();
		}
	}

	private bool CalculatePathAndTargets(AbilityData ability, CustomGridNodeBase targetNode, CustomGridNodeBase casterNode, out List<CustomGridNodeBase> pathNodes, out MechanicEntity[] targets)
	{
		MechanicEntity caster = ability.Caster;
		(OrientedPatternData Pattern, List<CustomGridNodeBase> Path) orientedPatternAndPath = GetOrientedPatternAndPath(ability, casterNode, targetNode);
		OrientedPatternData item = orientedPatternAndPath.Pattern;
		List<CustomGridNodeBase> item2 = orientedPatternAndPath.Path;
		pathNodes = item2;
		int limit = (DamageAllUnitsInLine ? int.MaxValue : caster.Size.GetLesserSide());
		targets = GetAllTargetUnits(item, caster, limit);
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
		movementAgent.MaxSpeedOverride = 10f;
		movementAgent.IsCharging = true;
		if (casterUnit != null)
		{
			casterUnit.State.IsCharging = true;
		}
		bool failedToStartPath = false;
		try
		{
			movementAgent.ForcePath(path);
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
	private MechanicEntity[] GetAllTargetUnits(OrientedPatternData pattern, MechanicEntity caster, int limit)
	{
		List<CustomGridNodeBase> list = pattern.Nodes.ToTempList();
		List<MechanicEntity> list2 = TempList.Get<MechanicEntity>();
		foreach (CustomGridNodeBase item in list)
		{
			foreach (CustomGridNodeBase occupiedNode in caster.GetOccupiedNodes(item.Vector3Position))
			{
				BaseUnitEntity unit = occupiedNode.GetUnit();
				if (unit != null && unit != caster && !unit.IsDeadOrUnconscious && !list2.Contains(unit) && (!IgnoreAllies || !caster.IsAlly(unit)) && (!IgnoreEnemies || !caster.IsEnemy(unit)))
				{
					limit--;
					list2.Add(unit);
					if (limit < 1)
					{
						break;
					}
				}
			}
			if (limit < 1)
			{
				break;
			}
		}
		return list2.ToArray();
	}

	public IComparer<CustomGridNodeBase> DistanceComparer(MechanicEntity caster)
	{
		return Comparer<CustomGridNodeBase>.Create((CustomGridNodeBase a, CustomGridNodeBase b) => Comparer<float>.Default.Compare(caster.DistanceToInCells(a.Vector3Position), caster.DistanceToInCells(b.Vector3Position)));
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public void OverridePattern(AoEPattern pattern)
	{
	}

	public OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool coveredTargetsOnly = false)
	{
		return GetOrientedPatternAndPath(ability, casterNode, targetNode, coveredTargetsOnly).Pattern;
	}

	public (OrientedPatternData Pattern, List<CustomGridNodeBase> Path) GetOrientedPatternAndPath(IAbilityDataProviderForPattern ability, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool coveredTargetsOnly = false)
	{
		MechanicEntity caster = ability.Caster;
		int rangeCells = ability.RangeCells;
		List<CustomGridNodeBase> path = GetPath(caster, casterNode, targetNode);
		if (path.Empty() || (!StepThroughTarget && !IsPathToTargetReachDestination(caster, targetNode, path)))
		{
			List<CustomGridNodeBase> list = TempList.Get<CustomGridNodeBase>();
			list.Add(targetNode);
			return (Pattern: new OrientedPatternData(list, casterNode), Path: TempList.Get<CustomGridNodeBase>());
		}
		CustomGridNodeBase customGridNodeBase = path[0];
		Vector2Int vector2Int = casterNode.CoordinatesInGrid - customGridNodeBase.CoordinatesInGrid;
		List<Vector2Int> list2 = (from i in caster.GetSortedNodesByDistanceToTarget(casterNode, targetNode.Vector3Position)
			select i.CoordinatesInGrid - casterNode.CoordinatesInGrid).ToTempList();
		CustomGridGraph g2 = (CustomGridGraph)customGridNodeBase.Graph;
		List<CustomGridNodeBase> list3 = TempList.Get<CustomGridNodeBase>();
		HashSet<CustomGridNodeBase> hashSet = TempHashSet.Get<CustomGridNodeBase>();
		foreach (CustomGridNodeBase item in path)
		{
			CustomGridNodeBase customGridNodeBase2 = GetNode(g2, item.CoordinatesInGrid + vector2Int);
			if (customGridNodeBase2 == null)
			{
				break;
			}
			int warhammerLength = CustomGraphHelper.GetWarhammerLength(customGridNodeBase2.CoordinatesInGrid - casterNode.CoordinatesInGrid);
			if (!StepThroughTarget && warhammerLength > rangeCells)
			{
				break;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			hashSet.Clear();
			foreach (Vector2Int item2 in list2)
			{
				hashSet.Add(GetNode(g2, customGridNodeBase2.CoordinatesInGrid + item2));
			}
			foreach (CustomGridNodeBase item3 in hashSet)
			{
				BaseUnitEntity unit = item3.GetUnit();
				flag3 |= unit != null && unit != caster && unit.IsConscious;
				if (unit != null)
				{
					flag |= flag3 && unit.IsEnemy(caster);
					flag2 |= flag3 && unit.IsAlly(caster);
				}
			}
			if (!coveredTargetsOnly || flag3)
			{
				list3.AddRange(hashSet);
			}
			if (!StepThroughTarget && StopOnFirstEncounter && ((!IgnoreAllies && flag2) || (!IgnoreEnemies && flag)))
			{
				break;
			}
		}
		if (IsCharge && !list3.Contains(targetNode))
		{
			list3.Add(targetNode);
		}
		return (Pattern: new OrientedPatternData(list3, list3.FirstItem()), Path: path);
		static CustomGridNodeBase GetNode(NavGraph g, Vector2Int i)
		{
			return ((CustomGridGraph)g).GetNode(i.x, i.y);
		}
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
		if (!CalculatePathAndTargets(ability, target.NearestNode, nearestNodeXZUnwalkable, out var pathNodes, out var targets))
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
						goto IL_00e3;
					}
				}
			}
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsInvalid;
			return false;
		}
		goto IL_00e3;
		IL_00e3:
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

	private List<CustomGridNodeBase> GetPath(MechanicEntity caster, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode)
	{
		if (!StepThroughTarget)
		{
			return GetPathToTarget(caster, casterNode, targetNode);
		}
		return GetStepThroughTargetPath(caster, casterNode, targetNode);
	}

	private List<CustomGridNodeBase> GetPathToTarget(MechanicEntity caster, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode)
	{
		BaseUnitEntity baseUnitEntity = targetNode.GetUnit();
		if (baseUnitEntity != null && baseUnitEntity.IsDeadOrUnconscious && CanTargetPoint)
		{
			baseUnitEntity = null;
		}
		return PathfindingService.Instance.FindPathChargeTB_Blocking(caster.MaybeMovementAgent, casterNode.Vector3Position, targetNode.Vector3Position, !StopOnFirstEncounter || IgnoreEnemies || IgnoreAllies, baseUnitEntity).path.Cast<CustomGridNodeBase>().ToTempList();
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
