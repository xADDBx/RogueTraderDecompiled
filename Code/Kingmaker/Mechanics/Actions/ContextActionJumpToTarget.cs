using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
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

	[SerializeField]
	[FormerlySerializedAs("Spell")]
	private BlueprintAbilityReference m_Spell;

	[SerializeField]
	private bool m_CastOnSelf;

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
		CustomGridNodeBase endPoint = GetEndNode(m_TargetPoint.GetValue(), base.Caster, base.Caster.Position);
		EventBus.RaiseEvent(delegate(IUnitGetAbilityJump h)
		{
			h.HandleUnitResultJump(startPoint.CellDistanceTo(endPoint), endPoint?.Vector3Position ?? m_TargetPoint.GetValue(), m_directJump, base.Caster, base.Caster, useAttack: false);
		});
		EventBus.RaiseEvent(delegate(IUnitJumpHandler h)
		{
			h.HandleUnitJump(startPoint.CellDistanceTo(endPoint), startPoint?.Vector3Position ?? m_TargetPoint.GetValue(), endPoint?.Vector3Position ?? m_TargetPoint.GetValue(), base.Caster, base.Context.SourceAbility);
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
		ItemEntityWeapon itemEntityWeapon = base.Context.SourceAbilityContext?.Ability?.Weapon;
		ItemEntityWeapon maybeWeapon = baseUnitEntity.Body.PrimaryHand.MaybeWeapon;
		ItemEntityWeapon maybeWeapon2 = baseUnitEntity.Body.SecondaryHand.MaybeWeapon;
		itemEntityWeapon = ((!UseSpecificWeaponClassification) ? (maybeWeapon ?? maybeWeapon2) : ((maybeWeapon?.Blueprint.Classification == Classification) ? maybeWeapon : maybeWeapon2));
		AnimationActionHandle animationActionHandle = base.Caster.MaybeAnimationManager?.CurrentAction;
		if (animationActionHandle != null && animationActionHandle.Action is UnitAnimationAction { Type: UnitAnimationType.LocoMotion })
		{
			animationActionHandle = null;
		}
		abilityData.OverrideWeapon = itemEntityWeapon;
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
		CustomGridNodeBase startPoint = casterPosition.GetNearestNodeXZ();
		CustomGridNodeBase nearestNodeXZ = targetPosition.GetNearestNodeXZ();
		int num = m_Cells.Calculate(base.Context);
		CustomGridNodeBase customGridNodeBase = (m_directJump ? targetPosition.GetNearestNodeXZ() : (m_FromPoint ? (targetPosition + (casterPosition - targetPosition).normalized * ((float)num * GraphParamsMechanicsCache.GridCellSize)).GetNearestNodeXZ() : (casterPosition + (targetPosition - casterPosition).normalized * ((float)num * GraphParamsMechanicsCache.GridCellSize)).GetNearestNodeXZ()));
		if (m_EndInTargetPoint && startPoint.CellDistanceTo(nearestNodeXZ) < num)
		{
			customGridNodeBase = nearestNodeXZ;
		}
		if (!caster.CanStandHere(customGridNodeBase) || (!CanJumpInPlace && customGridNodeBase == startPoint))
		{
			List<CustomGridNodeBase> list = new List<CustomGridNodeBase>();
			IntRect rect = customGridNodeBase.GetUnit()?.SizeRect ?? SizePathfindingHelper.GetRectForSize(Size.Medium);
			BaseUnitEntity unit = customGridNodeBase.GetUnit();
			foreach (CustomGridNodeBase item in GridAreaHelper.GetNodesSpiralAround(customGridNodeBase, rect, Math.Max(caster.SizeRect.Height, caster.SizeRect.Width)))
			{
				if (!caster.CanStandHere(item) || (!CanJumpInPlace && item == startPoint))
				{
					continue;
				}
				foreach (CustomGridNodeBase node in GridAreaHelper.GetNodes(item, caster.SizeRect))
				{
					if (unit == null)
					{
						if (node.CellDistanceTo(customGridNodeBase) <= 1)
						{
							list.Add(item);
							break;
						}
					}
					else if (unit.DistanceToInCells(node.Vector3Position) <= 1)
					{
						list.Add(item);
						break;
					}
				}
			}
			IOrderedEnumerable<CustomGridNodeBase> source = list.OrderBy((CustomGridNodeBase x) => startPoint.CellDistanceTo(x));
			customGridNodeBase = ((source.Count() > 1) ? source.FirstOrDefault((CustomGridNodeBase node) => node != caster.GetNearestNodeXZ()) : source.FirstOrDefault());
		}
		return customGridNodeBase;
	}

	public override bool IsValidToCast(TargetWrapper target, MechanicEntity caster, Vector3 casterPosition)
	{
		return GetEndNode(target.HasEntity ? target.Entity.Position : target.Point, caster, casterPosition) != null;
	}
}
