using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("d06159ad7d43437795acb8cb013e1e75")]
public class AbilityStepThroughTarget : AbilityCustomLogic, IAbilityAoEPatternProvider, IAbilityTargetRestriction
{
	public const bool StopOnFirstEncounter = true;

	public bool IgnoreEnemies;

	public bool IgnoreAllies;

	public bool DisableAttacksOfOpportunity;

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

	public bool ExcludeUnwalkable => false;

	TargetType IAbilityAoEPatternProvider.Targets => TargetType.Any;

	public AoEPattern Pattern => null;

	public override bool IsMoveUnit => true;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper targetWrapper)
	{
		MechanicEntity caster = context.Caster;
		UnitMovementAgentBase movementAgent = caster.MaybeMovementAgent;
		if (movementAgent == null)
		{
			PFLog.Default.Error("Movement agent is missing");
			yield break;
		}
		PartUnitCommands commandsOptional = caster.GetCommandsOptional();
		if (commandsOptional == null)
		{
			PFLog.Default.Error("Commands is missing");
			yield break;
		}
		CustomGridNodeBase nearestNodeXZUnwalkable = Game.Instance.VirtualPositionController.GetDesiredPosition(context.Ability.Caster).GetNearestNodeXZUnwalkable();
		CustomGridNodeBase nearestNodeXZUnwalkable2 = targetWrapper.Point.GetNearestNodeXZUnwalkable();
		OrientedPatternData orientedPattern = GetOrientedPattern(context.Ability, nearestNodeXZUnwalkable, nearestNodeXZUnwalkable2);
		List<CustomGridNodeBase> pathFromPattern = GetPathFromPattern(orientedPattern, caster, nearestNodeXZUnwalkable, nearestNodeXZUnwalkable2);
		CustomGridNodeBase lastMovementNode;
		BaseUnitEntity[] targetUnits = GetAllTargetUnits(pathFromPattern, caster, out lastMovementNode);
		if (pathFromPattern.Count == 0)
		{
			PFLog.Default.Error("Can't find path");
			yield break;
		}
		Buff buff = caster.Buffs.Add(BuffOnMovement, context, null);
		pathFromPattern.Insert(0, nearestNodeXZUnwalkable);
		ForcedPath forcedPath = ForcedPath.Construct(pathFromPattern);
		if (forcedPath.vectorPath.Count != 0)
		{
			UnitMoveToProperParams unitMoveToProperParams = new UnitMoveToProperParams(forcedPath, 0f);
			if (DisableAttacksOfOpportunity)
			{
				unitMoveToProperParams.DisableAttackOfOpportunity.Retain();
			}
			UnitCommandHandle moveCmdHandle = commandsOptional.AddToQueueFirst(unitMoveToProperParams);
			movementAgent.MaxSpeedOverride = 10f;
			movementAgent.IsCharging = true;
			while (!moveCmdHandle.IsFinished)
			{
				yield return null;
				if (!(moveCmdHandle.TimeSinceStart <= 5f))
				{
					moveCmdHandle.ForceFinishForTurnBased(AbstractUnitCommand.ResultType.Success);
					caster.Position = lastMovementNode.Vector3Position;
					PFLog.Default.ErrorWithReport("Move command takes too long time, force finished");
					break;
				}
			}
			movementAgent.IsCharging = false;
			movementAgent.MaxSpeedOverride = null;
		}
		using (context.GetDataScope(caster.ToITargetWrapper()))
		{
			ActionsOnCaster.Run();
		}
		BaseUnitEntity[] array = targetUnits;
		foreach (BaseUnitEntity baseUnitEntity in array)
		{
			using (context.GetDataScope(baseUnitEntity.ToITargetWrapper()))
			{
				ActionsOnEncounteredTarget.Run();
			}
			(caster as BaseUnitEntity)?.LookAt(baseUnitEntity.Position);
			yield return new AbilityDeliveryTarget(baseUnitEntity);
		}
		buff?.Remove();
	}

	private static BaseUnitEntity[] GetAllTargetUnits(List<CustomGridNodeBase> pathNodes, MechanicEntity caster, out CustomGridNodeBase lastMovementNode)
	{
		lastMovementNode = null;
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		for (int num = pathNodes.Count - 1; num >= 0; num--)
		{
			lastMovementNode = pathNodes[num];
			if (lastMovementNode.TryGetUnit(out var unit) && !unit.IsDeadOrUnconscious && unit != caster && !list.Contains(unit))
			{
				list.Add(unit);
			}
		}
		return list.ToArray();
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
		MechanicEntity caster = ability.Caster;
		return GetOrientedPattern(caster, casterNode, targetNode, coveredTargetsOnly);
	}

	private OrientedPatternData GetOrientedPattern(MechanicEntity caster, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool coveredTargetsOnly = false)
	{
		BaseUnitEntity unit = targetNode.GetUnit();
		CustomGridNodeBase customGridNodeBase = targetNode;
		if (unit != null && !unit.Size.Is1x1())
		{
			customGridNodeBase = unit.GetInnerNodeNearestToTarget(casterNode.Vector3Position);
		}
		Vector2 direction = (customGridNodeBase.Vector3Position - casterNode.Vector3Position).To2D().normalized;
		if (direction.sqrMagnitude < 0.1f)
		{
			direction = Vector2.up;
		}
		CustomGridGraph customGridGraph = (CustomGridGraph)casterNode.Graph;
		List<CustomGridNodeBase> list = TempList.Get<CustomGridNodeBase>();
		BaseUnitEntity baseUnitEntity = null;
		foreach (Vector2Int item in new Linecast.Ray2NodeOffsets(Vector2Int.zero, direction))
		{
			if (CustomGraphHelper.GetWarhammerLength(item) > 10)
			{
				break;
			}
			CustomGridNodeBase node = customGridGraph.GetNode(casterNode.XCoordinateInGrid + item.x, casterNode.ZCoordinateInGrid + item.y);
			BaseUnitEntity unit2 = node.GetUnit();
			bool flag = unit2 != null && unit2 != caster && unit2.LifeState.IsConscious;
			bool flag2 = flag && unit2.IsEnemy(caster);
			bool flag3 = flag && unit2.IsAlly(caster);
			if (node != null && list.Count > 0 && !node.ContainsConnection(list.Last()))
			{
				break;
			}
			if (!coveredTargetsOnly || flag)
			{
				list.Add(node);
			}
			bool flag4 = (!IgnoreAllies && flag3) || (!IgnoreEnemies && flag2);
			if ((!flag4 && node != casterNode) || (baseUnitEntity != null && baseUnitEntity != unit2))
			{
				break;
			}
			if (flag4)
			{
				baseUnitEntity = unit2;
			}
		}
		return new OrientedPatternData(list, casterNode);
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper targetWrapper, Vector3 casterPosition)
	{
		LocalizedString failReason;
		return CheckTargetRestriction(ability.Caster, targetWrapper, casterPosition, out failReason);
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper targetWrapper, Vector3 casterPosition)
	{
		CheckTargetRestriction(ability.Caster, targetWrapper, casterPosition, out var failReason);
		return failReason;
	}

	private bool CheckTargetRestriction(MechanicEntity caster, TargetWrapper targetWrapper, Vector3 casterPosition, [CanBeNull] out LocalizedString failReason)
	{
		CustomGridNodeBase nearestNodeXZUnwalkable = Game.Instance.VirtualPositionController.GetDesiredPosition(caster).GetNearestNodeXZUnwalkable();
		CustomGridNodeBase nearestNodeXZUnwalkable2 = targetWrapper.Point.GetNearestNodeXZUnwalkable();
		OrientedPatternData orientedPattern = GetOrientedPattern(caster, nearestNodeXZUnwalkable, nearestNodeXZUnwalkable2);
		List<CustomGridNodeBase> pathFromPattern = GetPathFromPattern(orientedPattern, caster, nearestNodeXZUnwalkable, nearestNodeXZUnwalkable2);
		if (pathFromPattern.Count == 0)
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsTooClose;
			return false;
		}
		if (pathFromPattern.Last().TryGetUnit(out var _))
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsInvalid;
			return false;
		}
		BaseUnitEntity baseUnitEntity = null;
		for (int num = 0; num < pathFromPattern.Count; num++)
		{
			if (!pathFromPattern[num].TryGetUnit(out var unit2) && num != pathFromPattern.Count - 1)
			{
				failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsTooFar;
				return false;
			}
			int num2;
			int num3;
			if (unit2 != null && unit2 != caster)
			{
				num2 = (unit2.LifeState.IsConscious ? 1 : 0);
				if (num2 != 0)
				{
					num3 = (unit2.IsEnemy(caster) ? 1 : 0);
					goto IL_00e8;
				}
			}
			else
			{
				num2 = 0;
			}
			num3 = 0;
			goto IL_00e8;
			IL_00e8:
			bool flag = (byte)num3 != 0;
			bool flag2 = num2 != 0 && unit2.IsAlly(caster);
			if ((!IgnoreAllies && flag2) || (!IgnoreEnemies && flag))
			{
				if (baseUnitEntity != null && baseUnitEntity != unit2)
				{
					failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsInvalid;
					return false;
				}
				baseUnitEntity = unit2;
			}
		}
		if (baseUnitEntity == null)
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsInvalid;
			return false;
		}
		failReason = null;
		return true;
	}

	private List<CustomGridNodeBase> GetPathFromPattern(OrientedPatternData pattern, MechanicEntity caster, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode)
	{
		Vector2 normalized = (targetNode.Vector3Position - casterNode.Vector3Position).To2D().normalized;
		if (normalized.sqrMagnitude < 1E-06f)
		{
			return Enumerable.Empty<CustomGridNodeBase>().ToTempList();
		}
		Linecast.Ray2NodeOffsets offsets = new Linecast.Ray2NodeOffsets(casterNode.CoordinatesInGrid, normalized);
		return new Linecast.Ray2Nodes((CustomGridGraph)casterNode.Graph, in offsets).TakeWhile((CustomGridNodeBase i) => pattern.Contains(i) || i == casterNode).ToTempList();
	}
}
