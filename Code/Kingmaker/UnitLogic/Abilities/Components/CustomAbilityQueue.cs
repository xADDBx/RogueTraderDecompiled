using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("a2cb91a2b5d142648acab0e10a1bc6f1")]
public class CustomAbilityQueue : AbilityCustomLogic
{
	[Space(8f)]
	public bool CastOnCasterInsteadOfInitialTarget;

	[FormerlySerializedAs("m_AbilityToCast")]
	public BlueprintAbilityReference AbilityToCast;

	[FormerlySerializedAs("m_QueConditions")]
	[Space(8f)]
	[Tooltip("If conditions are passed after resolving first ability the next one gets queued")]
	public ConditionsChecker QueConditions;

	[FormerlySerializedAs("m_AbilityToQue")]
	public BlueprintAbilityReference AbilityToQue;

	[HideIf("CastOnCasterInsteadOfInitialTarget")]
	public bool CastOnRandomTarget;

	[ShowIf("CastOnRandomTarget")]
	public ContextValue Range;

	[ShowIf("CastOnRandomTarget")]
	public ConditionsChecker ConditionsOnTarget;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		PartUnitCommands commands = context.Caster.GetCommandsOptional();
		if (commands == null)
		{
			yield break;
		}
		AbilityData ability = CreateAbility(AbilityToCast, context);
		TargetWrapper targetWrapper = (CastOnCasterInsteadOfInitialTarget ? ((TargetWrapper)context.Caster) : target);
		if (CastOnRandomTarget)
		{
			List<MechanicEntity> list = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity p) => !p.Features.IsUntargetable && !p.LifeState.IsDead && p.IsInCombat && IsConditionPassed(ConditionsOnTarget, context, p) && context.Caster.InRangeInCells(p, Range.Calculate(context))).Cast<MechanicEntity>().ToList();
			targetWrapper = list.Random(PFStatefulRandom.Mechanics);
		}
		UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(ability, targetWrapper)
		{
			FreeAction = true
		};
		UnitCommandHandle cmdHandle = commands.AddToQueue(cmdParams);
		while (!cmdHandle.IsFinished)
		{
			yield return null;
		}
		if (IsConditionPassed(QueConditions, context, context.Caster))
		{
			UnitUseAbilityParams cmdParams2 = new UnitUseAbilityParams(CreateAbility(AbilityToQue, context), targetWrapper)
			{
				FreeAction = true
			};
			UnitCommandHandle nextCmdHandle = commands.AddToQueue(cmdParams2);
			while (!nextCmdHandle.IsFinished)
			{
				yield return null;
			}
		}
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	private AbilityData CreateAbility(BlueprintAbilityReference ability, AbilityExecutionContext context)
	{
		return new AbilityData(ability, context.Caster)
		{
			OverrideWeapon = context.Ability.Weapon
		};
	}

	private bool IsConditionPassed(ConditionsChecker conditions, MechanicsContext context, MechanicEntity entity)
	{
		using (context.GetDataScope(entity.ToITargetWrapper()))
		{
			return conditions.Check();
		}
	}
}
