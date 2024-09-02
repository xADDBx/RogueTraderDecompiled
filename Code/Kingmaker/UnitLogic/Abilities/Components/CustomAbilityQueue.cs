using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
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
	[FormerlySerializedAs("m_AbilityToCast")]
	public BlueprintAbilityReference AbilityToCast;

	public bool CastOnCasterInsteadOfInitialTarget;

	[FormerlySerializedAs("m_QueConditions")]
	[Space(8f)]
	[Tooltip("If conditions are passed after resolving first ability the next one gets queued")]
	public ConditionsChecker QueConditions;

	[FormerlySerializedAs("m_AbilityToQue")]
	public BlueprintAbilityReference AbilityToQue;

	[Space(8f)]
	[HideIf("CastOnCasterInsteadOfInitialTarget")]
	public bool QueOnRandomTarget;

	[ShowIf("QueOnRandomTarget")]
	public ContextValue Range;

	[ShowIf("QueOnRandomTarget")]
	public ConditionsChecker ConditionsOnTarget;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		PartUnitCommands commands = context.Caster.GetCommandsOptional();
		if (commands == null)
		{
			yield break;
		}
		UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(CreateAbility(AbilityToCast, context), CastOnCasterInsteadOfInitialTarget ? ((TargetWrapper)context.Caster) : target)
		{
			FreeAction = true
		};
		UnitCommandHandle cmdHandle = commands.AddToQueue(cmdParams);
		AbilityExecutionProcess executionProcess = null;
		while (!cmdHandle.IsFinished && (executionProcess == null || !executionProcess.IsStarted))
		{
			executionProcess = ((UnitUseAbility)cmdHandle.Cmd)?.ExecutionProcess;
			yield return null;
		}
		if (executionProcess != null)
		{
			while (!executionProcess.IsEnded)
			{
				yield return null;
			}
		}
		if (!IsConditionPassed(QueConditions, context, context.Caster))
		{
			yield break;
		}
		TargetWrapper targetWrapper = target;
		if (QueOnRandomTarget)
		{
			targetWrapper = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity p) => !p.Features.IsUntargetable && !p.LifeState.IsDead && p.IsInCombat && IsConditionPassed(ConditionsOnTarget, context, p) && context.Caster.InRangeInCells(p, Range.Calculate(context))).Cast<MechanicEntity>().ToList()
				.Random(PFStatefulRandom.Mechanics);
		}
		AbilityData abilityData = CreateAbility(AbilityToQue, context);
		if (!(targetWrapper != null) || !abilityData.IsValid(targetWrapper))
		{
			yield break;
		}
		UnitUseAbilityParams cmdParams2 = new UnitUseAbilityParams(abilityData, targetWrapper)
		{
			FreeAction = true
		};
		UnitCommandHandle nextCmdHandle = commands.AddToQueue(cmdParams2);
		AbilityExecutionProcess newExecutionProcess = null;
		while (!nextCmdHandle.IsFinished && (newExecutionProcess == null || !newExecutionProcess.IsStarted))
		{
			newExecutionProcess = ((UnitUseAbility)nextCmdHandle.Cmd)?.ExecutionProcess;
			yield return null;
		}
		if (newExecutionProcess != null)
		{
			while (!newExecutionProcess.IsEnded)
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
