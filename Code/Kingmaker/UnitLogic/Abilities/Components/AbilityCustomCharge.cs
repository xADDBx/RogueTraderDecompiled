using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.Pathfinding;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("fc54b44f7e2049e5b3007a7f600c6e99")]
public class AbilityCustomCharge : AbilityCustomLogic, IAbilityTargetRestriction, IAbilityCustomAnimation
{
	public override bool IsEngageUnit => true;

	public override bool IsMoveUnit => true;

	public int MinRangeCells => ((BlueprintAbility)base.OwnerBlueprint).MinRange;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper targetWrapper)
	{
		MechanicEntity target = targetWrapper.Entity;
		if (target == null)
		{
			PFLog.Default.Error("Target unit is missing");
			yield break;
		}
		if (!(context.Caster is UnitEntity caster))
		{
			PFLog.Default.Error("Caster unit is missing");
			yield break;
		}
		if (caster.GetThreatHandMelee() == null)
		{
			PFLog.Default.Error("Invalid caster's weapon");
			yield break;
		}
		if (caster.View.AnimationManager?.CurrentAction is UnitAnimationActionHandle unitAnimationActionHandle)
		{
			unitAnimationActionHandle.DoesNotPreventMovement = true;
		}
		else
		{
			PFLog.Default.Error("No animation handle found");
		}
		caster.View.StopMoving();
		caster.View.MovementAgent.IsCharging = true;
		using PathDisposable<WarhammerPathPlayer> pd = PathfindingService.Instance.FindPathTB_Delayed(caster.View.MovementAgent, target.Position, limitRangeByActionPoints: false, 1, this);
		WarhammerPathPlayer path = pd.Path;
		while (!path.IsDoneAndPostProcessed())
		{
			yield return null;
		}
		int num = path.vectorPath.TakeWhile((Vector3 v) => !target.InRangeInCells(v, context.Ability.Weapon.AttackRange)).Count();
		IEnumerable<Vector3> source = path.vectorPath.Take(num + 1);
		caster.View.MovementAgent.ForcePath(ForcedPath.Construct(source.ToList()), disableApproachRadius: true);
		caster.Buffs.Add(BlueprintRoot.Instance.SystemMechanics.ChargeBuff, context, 1.Rounds().Seconds);
		caster.State.IsCharging = true;
		while (caster.View.MovementAgent.IsReallyMoving)
		{
			yield return null;
		}
		AbilityDeliverAttackWithWeapon abilityDeliverAttackWithWeapon = new AbilityDeliverAttackWithWeapon();
		IEnumerator<AbilityDeliveryTarget> attackEnum = abilityDeliverAttackWithWeapon.Deliver(context, target);
		while (attackEnum.MoveNext())
		{
			yield return attackEnum.Current;
		}
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

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper targetWrapper, Vector3 casterPosition)
	{
		LocalizedString failReason;
		return CheckTargetRestriction(ability.Caster, targetWrapper, casterPosition, out failReason);
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		CheckTargetRestriction(ability.Caster, target, casterPosition, out var failReason);
		return failReason;
	}

	private bool CheckTargetRestriction(MechanicEntity caster, TargetWrapper targetWrapper, Vector3 casterPosition, [CanBeNull] out LocalizedString failReason)
	{
		MechanicEntity entity = targetWrapper.Entity;
		if (entity == null)
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsInvalid;
			return false;
		}
		if (entity.DistanceToInCells(casterPosition) < MinRangeCells)
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsTooClose;
			return false;
		}
		if (ObstacleAnalyzer.TraceAlongNavmesh(casterPosition, entity.Position) != entity.Position)
		{
			failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.ObstacleBetweenCasterAndTarget;
			return false;
		}
		if (caster is UnitEntity unitEntity && !unitEntity.View.MovementAgent.AvoidanceDisabled)
		{
			float valueOrDefault = (caster.Corpulence + entity.Corpulence + (float?)unitEntity.GetFirstWeapon()?.AttackRange).GetValueOrDefault();
			Vector2 normalized = (entity.Position - casterPosition).To2D().normalized;
			Vector2 vector = entity.Position.To2D() - normalized * valueOrDefault;
			foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
			{
				if (allBaseAwakeUnit != caster && allBaseAwakeUnit != entity && (bool)allBaseAwakeUnit.View && !allBaseAwakeUnit.View.MovementAgent.AvoidanceDisabled && (float)(int)(vector - allBaseAwakeUnit.Position.To2D()).magnitude < (caster.Corpulence + allBaseAwakeUnit.Corpulence) * 0.8f)
				{
					failReason = BlueprintRoot.Instance.LocalizedTexts.Reasons.ObstacleBetweenCasterAndTarget;
					return false;
				}
			}
		}
		failReason = null;
		return true;
	}

	public UnitAnimationActionLink GetAbilityAction(BaseUnitEntity caster)
	{
		return null;
	}
}
