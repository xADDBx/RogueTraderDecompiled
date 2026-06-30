using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Controllers.Units;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Mechanics.Actions;

[TypeId("330ce332a2f8456690072cf514b8529c")]
public class ContextActionJumpToTarget : ContextActionMove
{
	[SerializeField]
	private ContextValue m_Cells;

	[SerializeField]
	private bool m_EndInTargetPoint;

	[SerializeField]
	private bool m_FromPoint;

	[SerializeField]
	private bool m_directJump;

	[SerializeField]
	private bool CanJumpInPlace;

	[KDB("Поведение в случае, если целевая точка занята: по-умолчанию ищется ближайшая незанятая клетка. С включенной галочкой ищется ближайшая незанятая клетка, которая еще будет соединена напрямую по навмешу с целевой.")]
	[SerializeField]
	private bool m_TryTakeConnectedNode;

	[SerializeField]
	[FormerlySerializedAs("Spell")]
	private BlueprintAbilityReference m_Spell;

	[SerializeField]
	private bool m_CastOnSelf;

	[SerializeField]
	private UnitAnimationJumpSubType m_JumpSubType;

	[InfoBox("Used only when animation doesn't exist or animation action has Looped Fly flag set.")]
	[SerializeField]
	private float m_Speed = 5f;

	public bool OverrideWeaponOfTheSpellWithCurrentWeapon;

	public bool UseSpecificWeaponClassification;

	[ShowIf("UseSpecificWeaponClassification")]
	public WeaponClassification Classification;

	public BlueprintAbility Spell => m_Spell?.Get();

	public override string GetCaption()
	{
		return $"Jump direct to {m_TargetPoint}";
	}

	protected override void RunAction()
	{
		CustomGridNodeBase startPoint = base.Caster.Position.GetNearestNodeXZ();
		Vector3 targetPointValue = m_TargetPoint.GetValue();
		CustomGridNodeBase endPoint = GetEndNode(targetPointValue, base.Caster, base.Caster.Position);
		UnitJumpMoveController.TryStartJump(base.Caster, startPoint.CellDistanceTo(endPoint), endPoint?.Vector3Position ?? targetPointValue, m_directJump, m_JumpSubType, m_Speed);
		EventBus.RaiseEvent(delegate(IUnitJumpHandler h)
		{
			h.HandleUnitJump(startPoint.CellDistanceTo(endPoint), startPoint?.Vector3Position ?? targetPointValue, endPoint?.Vector3Position ?? targetPointValue, base.Caster, base.Context.SourceAbility);
		});
		if (Spell == null)
		{
			return;
		}
		if (!(base.Caster is BaseUnitEntity baseUnitEntity))
		{
			Element.LogError(this, "Caster is missing");
			return;
		}
		PartUnitCommands commandsOptional = baseUnitEntity.GetCommandsOptional();
		AbilityData abilityData = CreateAbility(m_Spell, base.Context.SourceAbilityContext);
		ItemEntityWeapon maybeWeapon = baseUnitEntity.Body.PrimaryHand.MaybeWeapon;
		ItemEntityWeapon maybeWeapon2 = baseUnitEntity.Body.SecondaryHand.MaybeWeapon;
		ItemEntityWeapon overrideWeapon = ((!UseSpecificWeaponClassification) ? (maybeWeapon ?? maybeWeapon2) : ((maybeWeapon?.Blueprint.Classification == Classification) ? maybeWeapon : maybeWeapon2));
		AnimationActionHandle animationActionHandle = base.Caster.MaybeAnimationManager?.CurrentAction;
		if (animationActionHandle != null && animationActionHandle.Action is UnitAnimationAction { Type: UnitAnimationType.LocoMotion })
		{
			animationActionHandle = null;
		}
		abilityData.OverrideWeapon = overrideWeapon;
		UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(abilityData, m_CastOnSelf ? ((TargetWrapper)baseUnitEntity) : base.Context.MainTarget)
		{
			FreeAction = true,
			OverrideAnimationHandle = animationActionHandle
		};
		commandsOptional?.AddToQueue(cmdParams);
	}

	private AbilityData CreateAbility(BlueprintAbilityReference ability, AbilityExecutionContext context)
	{
		return new AbilityData(ability, context.Caster)
		{
			OverrideWeapon = context.Ability.Weapon
		};
	}

	private CustomGridNodeBase GetEndNode(Vector3 targetPosition, MechanicEntity caster, Vector3 casterPosition)
	{
		CustomGridNodeBase nearestNodeXZ = casterPosition.GetNearestNodeXZ();
		CustomGridNodeBase nearestNodeXZ2 = targetPosition.GetNearestNodeXZ();
		int num = m_Cells.Calculate(base.Context);
		CustomGridNodeBase customGridNodeBase = (m_directJump ? nearestNodeXZ2 : (m_FromPoint ? (targetPosition + (casterPosition - targetPosition).normalized * ((float)num * GraphParamsMechanicsCache.GridCellSize)).GetNearestNodeXZ() : (casterPosition + (targetPosition - casterPosition).normalized * ((float)num * GraphParamsMechanicsCache.GridCellSize)).GetNearestNodeXZ()));
		if (m_EndInTargetPoint && nearestNodeXZ.CellDistanceTo(nearestNodeXZ2) < num)
		{
			customGridNodeBase = nearestNodeXZ2;
		}
		if (!CanLandOn(caster, nearestNodeXZ, customGridNodeBase))
		{
			customGridNodeBase = FindAvailableEndNode(caster, nearestNodeXZ, customGridNodeBase);
		}
		return customGridNodeBase;
	}

	private CustomGridNodeBase FindAvailableEndNode(MechanicEntity caster, CustomGridNodeBase startPoint, CustomGridNodeBase targetNode)
	{
		CustomGridNodeBase nearestNodeXZ = caster.GetNearestNodeXZ();
		BaseUnitEntity unit = targetNode.GetUnit();
		IntRect rect = unit?.SizeRect ?? SizePathfindingHelper.GetRectForSize(Size.Medium);
		NodeList targetNodes = (m_TryTakeConnectedNode ? GridAreaHelper.GetNodes(targetNode, rect) : NodeList.Empty);
		bool flag = false;
		CustomGridNodeBase customGridNodeBase = null;
		CustomGridNodeBase customGridNodeBase2 = null;
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		foreach (CustomGridNodeBase item in GridAreaHelper.GetNodesSpiralAround(targetNode, rect, Math.Max(caster.SizeRect.Height, caster.SizeRect.Width)))
		{
			if (!CanLandOn(caster, startPoint, item))
			{
				continue;
			}
			NodeList nodes = GridAreaHelper.GetNodes(item, caster.SizeRect);
			if (!IsAdjacentToTarget(nodes, unit, targetNode))
			{
				continue;
			}
			if (item == nearestNodeXZ)
			{
				flag = true;
				continue;
			}
			float sqrMagnitude = (startPoint.Vector3Position - item.Vector3Position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				customGridNodeBase = item;
				num = sqrMagnitude;
			}
			if (m_TryTakeConnectedNode && sqrMagnitude < num2 && IsConnectedToTarget(nodes, targetNodes))
			{
				customGridNodeBase2 = item;
				num2 = sqrMagnitude;
			}
		}
		CustomGridNodeBase customGridNodeBase3 = customGridNodeBase2;
		if (customGridNodeBase3 == null)
		{
			customGridNodeBase3 = customGridNodeBase;
			if (customGridNodeBase3 == null)
			{
				if (!flag)
				{
					return null;
				}
				customGridNodeBase3 = nearestNodeXZ;
			}
		}
		return customGridNodeBase3;
	}

	private static bool IsAdjacentToTarget(NodeList candidateNodes, MechanicEntity unitAtTarget, CustomGridNodeBase targetNode)
	{
		foreach (CustomGridNodeBase item in candidateNodes)
		{
			if ((unitAtTarget == null) ? (item.CellDistanceTo(targetNode) <= 1) : (unitAtTarget.DistanceToInCells(item.Vector3Position) <= 1))
			{
				return true;
			}
		}
		return false;
	}

	private bool CanLandOn(MechanicEntity caster, CustomGridNodeBase startPoint, CustomGridNodeBase node)
	{
		if (caster.CanStandHere(node) && (CanJumpInPlace || node != startPoint))
		{
			return startPoint.Area == node.Area;
		}
		return false;
	}

	public override bool IsValidToCast(TargetWrapper target, MechanicEntity caster, Vector3 casterPosition)
	{
		return GetEndNode(target.HasEntity ? target.Entity.Position : target.Point, caster, casterPosition) != null;
	}

	private static bool IsConnectedToTarget(NodeList candidateNodes, NodeList targetNodes)
	{
		foreach (CustomGridNodeBase item in candidateNodes)
		{
			foreach (CustomGridNodeBase item2 in targetNodes)
			{
				if (item.ContainsConnection(item2))
				{
					return true;
				}
			}
		}
		return false;
	}
}
