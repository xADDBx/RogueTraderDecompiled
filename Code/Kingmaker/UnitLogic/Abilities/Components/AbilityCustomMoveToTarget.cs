using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("e627a85beb904bf2bd6608ecdbcbadbc")]
public class AbilityCustomMoveToTarget : AbilityCustomLogic, IAbilityTargetRestriction
{
	[SerializeField]
	private bool m_OverrideMaxSpeed;

	[SerializeField]
	[ShowIf("m_OverrideMaxSpeed")]
	private float m_MaxSpeedOverride = 10f;

	[SerializeField]
	private ActionList m_ActionsOnTargetAfterMoved;

	[SerializeField]
	private ActionList m_ActionsAfterMoved;

	public bool DisableAttacksOfOpportunity;

	[ShowIf("DisableAttacksOfOpportunity")]
	public bool DisableOnlyIfHasFact;

	[SerializeField]
	private bool m_PassThroughAllUnits;

	[InfoBox("Если стоит CasterMustStandInTarget, кастер должен мочь встать в клетку, иначе кастовать нельзя")]
	[HideIf("m_CasterMustStandNearTarget")]
	[SerializeField]
	private bool m_CasterMustStandInTarget;

	[InfoBox("Если стоит CasterMustStandNearTarget, около цели должна быть свободная клетка для кастера, иначе кастовать нельзя")]
	[HideIf("m_CasterMustStandInTarget")]
	[SerializeField]
	private bool m_CasterMustStandNearTarget;

	public bool IgnoreObstacles;

	public bool AllowNotStraightMovement;

	[ShowIf("DisableOnlyIfHasFact")]
	[SerializeField]
	[FormerlySerializedAs("CheckedFact")]
	private BlueprintUnitFactReference m_CheckedFact;

	public override bool IsMoveUnit => true;

	public BlueprintUnitFact CheckedFact => m_CheckedFact?.Get();

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper targetWrapper)
	{
		MechanicEntity caster2 = context.Caster;
		if (!(caster2 is UnitEntity caster))
		{
			PFLog.Default.Error("Caster unit is missing");
			yield break;
		}
		PartUnitCommands commands = caster.GetCommandsOptional();
		if (commands == null)
		{
			PFLog.Default.Error("Commands is missing");
			yield break;
		}
		UnitMovementAgentBase maybeMovementAgent = caster.MaybeMovementAgent;
		if (maybeMovementAgent == null)
		{
			PFLog.Default.Error("Caster movement agent is missing");
			yield break;
		}
		caster.View.StopMoving();
		maybeMovementAgent.IsCharging = true;
		maybeMovementAgent.MaxSpeedOverride = (m_OverrideMaxSpeed ? new float?(m_MaxSpeedOverride) : null);
		caster.State.IsCharging = true;
		TargetWrapper target = targetWrapper;
		if (TryGetExplicitTargetNode(caster, targetWrapper, out var result))
		{
			target = new TargetWrapper(result.Vector3Position);
		}
		if (m_PassThroughAllUnits)
		{
			caster.Features.CanPassThroughUnits.Retain();
		}
		using PathDisposable<WarhammerPathPlayer> pd = PathfindingService.Instance.FindPathTB_Delayed(caster.MaybeMovementAgent, target, limitRangeByActionPoints: false, 1, this);
		WarhammerPathPlayer path = pd.Path;
		while (!path.IsDoneAndPostProcessed())
		{
			yield return null;
		}
		if (m_PassThroughAllUnits)
		{
			caster.Features.CanPassThroughUnits.Release();
		}
		if (path.error || path.vectorPath.Count < 2)
		{
			PFLog.Default.Error("Can't find path");
			yield break;
		}
		path.OverrideBlockMode(BlockMode.Ignore);
		if (!targetWrapper.IsPoint && path.vectorPath.Count > 0 && path.vectorPath.Last().GetNearestNodeXZ().TryGetUnit(out var unit) && unit != null)
		{
			path.vectorPath.RemoveAt(path.vectorPath.Count - 1);
		}
		UnitMoveToProperParams unitMoveToProperParams = new UnitMoveToProperParams(ForcedPath.Construct(path), 0f);
		if (DisableAttacksOfOpportunity && (!DisableOnlyIfHasFact || caster.Facts.Contains(CheckedFact)))
		{
			unitMoveToProperParams.DisableAttackOfOpportunity.Retain();
		}
		UnitCommandHandle moveCmdHandle = commands.AddToQueueFirst(unitMoveToProperParams);
		while (!moveCmdHandle.IsFinished)
		{
			yield return null;
			if (!(moveCmdHandle.TimeSinceStart <= 5f))
			{
				moveCmdHandle.ForceFinishForTurnBased(AbstractUnitCommand.ResultType.Success);
				caster.Position = path.vectorPath.Last();
				PFLog.Default.ErrorWithReport("Move command takes too long time, force finished");
				break;
			}
		}
		if (targetWrapper.Entity is UnitEntity entity)
		{
			using (context.GetDataScope(entity.ToITargetWrapper()))
			{
				m_ActionsOnTargetAfterMoved?.Run();
				EventBus.RaiseEvent(delegate(IUnitMovedByAbilityHandler h)
				{
					h.HandleUnitMovedByAbility(context, path.path.Count - 1);
				});
			}
		}
		m_ActionsAfterMoved?.Run();
		caster.CombatState.RegisterMoveCells(path.path.Count - 1);
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

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		LocalizedString failReason;
		return CheckTargetRestriction(ability.Caster, target, casterPosition, out failReason);
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		CheckTargetRestriction(ability.Caster, target, casterPosition, out var failReason);
		return failReason;
	}

	private bool CheckTargetRestriction(MechanicEntity caster, TargetWrapper targetWrapper, Vector3 casterPosition, [CanBeNull] out LocalizedString failReason)
	{
		IntRect toSize;
		Vector3 forward;
		if (targetWrapper.Entity == null)
		{
			toSize = default(IntRect);
			forward = Vector3.forward;
		}
		else
		{
			toSize = targetWrapper.Entity.SizeRect;
			forward = targetWrapper.Entity.Forward;
		}
		if (WarhammerGeometryUtils.DistanceToInCells(casterPosition, caster.SizeRect, caster.Forward, targetWrapper.Point, toSize, forward) < ((BlueprintAbility)base.OwnerBlueprint).MinRange)
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsTooClose;
			return false;
		}
		if (!AllowNotStraightMovement && (ObstacleAnalyzer.TraceAlongNavmesh(casterPosition, targetWrapper.Point) - targetWrapper.Point).sqrMagnitude >= 1E-08f && !IgnoreObstacles)
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.ObstacleBetweenCasterAndTarget;
			return false;
		}
		if (AllowNotStraightMovement || m_CasterMustStandInTarget || m_CasterMustStandNearTarget)
		{
			if (!TryGetExplicitTargetNode(caster, targetWrapper, out var result))
			{
				failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.PathBlocked;
				return false;
			}
			List<CustomGridNodeBase> pathToTarget = GetPathToTarget(caster, casterPosition.GetNearestNodeXZ(), (CustomGridNodeBase)result);
			if (pathToTarget[pathToTarget.Count - 1] != result)
			{
				failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.PathBlocked;
				return false;
			}
		}
		failReason = null;
		return true;
	}

	private bool TryGetExplicitTargetNode(MechanicEntity caster, TargetWrapper targetWrapper, out GraphNode result)
	{
		result = null;
		IntRect rect = ((targetWrapper.Entity != null) ? targetWrapper.Entity.SizeRect : default(IntRect));
		if (m_CasterMustStandInTarget)
		{
			CustomGridNodeBase nearestNodeXZUnwalkable = targetWrapper.Point.GetNearestNodeXZUnwalkable();
			if (caster.CanStandHere(nearestNodeXZUnwalkable))
			{
				result = nearestNodeXZUnwalkable;
				return true;
			}
		}
		else if (m_CasterMustStandNearTarget)
		{
			CustomGridNodeBase nearestNodeXZUnwalkable2 = targetWrapper.Point.GetNearestNodeXZUnwalkable();
			foreach (CustomGridNodeBase item in GridAreaHelper.GetNodesSpiralAround(nearestNodeXZUnwalkable2, rect, Math.Max(caster.SizeRect.Height, caster.SizeRect.Width)))
			{
				if (!caster.CanStandHere(item))
				{
					continue;
				}
				foreach (CustomGridNodeBase node in GridAreaHelper.GetNodes(item, caster.SizeRect))
				{
					if (node.CellDistanceTo(nearestNodeXZUnwalkable2) <= 1)
					{
						result = item;
						return true;
					}
				}
			}
		}
		return false;
	}

	private List<CustomGridNodeBase> GetPathToTarget(MechanicEntity caster, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode)
	{
		return PathfindingService.Instance.FindPathTB_Blocking_Cached(caster.MaybeMovementAgent, casterNode.Vector3Position, targetNode.Vector3Position, Mathf.Max(-1, ((BlueprintAbility)base.OwnerBlueprint).GetRange() * 2), ignoreThreateningAreaCost: false, m_PassThroughAllUnits).path.Cast<CustomGridNodeBase>().ToTempList();
	}
}
