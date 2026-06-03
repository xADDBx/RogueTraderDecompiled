using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.StatefulRandom;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("32c217123d704c0385778db3f29d2496")]
public class AbilityMultiTarget : AbilityDeliverEffect, IAbilityMultiTarget, IAbilityGetAllTargetsForTooltip, IAbilityGetTooltipTarget
{
	public enum CasterType
	{
		Caster,
		PetOfCaster
	}

	[Serializable]
	public class TargetItem
	{
		[Tooltip("Caster to use for targeting purposes (UI).")]
		public CasterType Caster;

		[Tooltip("Ability to use for selecting the target only (UI).")]
		public BlueprintAbilityReference Ability;
	}

	[Serializable]
	public class QueueItem
	{
		[Tooltip("Caster to use for actually running the ability.")]
		public CasterType Caster;

		[Tooltip("Ability to run.")]
		public BlueprintAbilityReference Ability;

		[Tooltip("Index of target to use, starting from 0 (which can be ability itself, if Use Self As First Target is true)")]
		public int TargetIndex;
	}

	[Tooltip("If set, uses this ability for selecting the first target (index 0).")]
	[SerializeField]
	private bool m_UseSelfAsFirstTarget = true;

	[Tooltip("Which target to use for the effects of this ability.")]
	[SerializeField]
	private int m_SelfTargetIndex;

	[Tooltip("A list of additional targets for the player to select (UI).")]
	[SerializeField]
	private List<TargetItem> m_Targets;

	[Tooltip("Abilities to execute")]
	[SerializeField]
	private List<QueueItem> m_AbilityQueue;

	public override bool IsEngageUnit => true;

	public int TargetAbilityCount => m_Targets.Count;

	public bool TryGetNextTargetAbility(AbilityData rootAbility, int targetIndex, out AbilityData ability)
	{
		BlueprintAbility blueprintAbility = null;
		MechanicEntity mechanicEntity = null;
		ability = null;
		if (m_UseSelfAsFirstTarget && targetIndex <= 0)
		{
			blueprintAbility = rootAbility.Blueprint;
			mechanicEntity = rootAbility.Caster;
		}
		else
		{
			if (m_UseSelfAsFirstTarget)
			{
				targetIndex--;
			}
			if (targetIndex >= 0 && targetIndex < m_Targets.Count)
			{
				TargetItem targetItem = m_Targets[targetIndex];
				blueprintAbility = targetItem.Ability.Get() ?? rootAbility.Blueprint;
				mechanicEntity = GetDelegateUnit(rootAbility.Caster, targetItem.Caster);
			}
		}
		if (blueprintAbility != null && mechanicEntity != null)
		{
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				ability = new AbilityData(blueprintAbility, mechanicEntity);
			}
		}
		return ability != null;
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		List<TargetWrapper> targets = context.AllTargets;
		if (targets == null || targets.Count == 0)
		{
			targets = new List<TargetWrapper> { target };
		}
		yield return new AbilityDeliveryTarget(GetTargetByIndexSafe(targets, m_SelfTargetIndex));
		foreach (QueueItem item in m_AbilityQueue)
		{
			BaseUnitEntity actualCaster = GetDelegateUnit(context.Caster, item.Caster);
			if (actualCaster == null)
			{
				PFLog.Default.Error(context.AbilityBlueprint, "Actual Caster is null");
				continue;
			}
			PartUnitCommands commands = actualCaster.GetCommandsOptional();
			if (commands == null)
			{
				PFLog.Default.Error(context.AbilityBlueprint, "Actual Caster has no commands");
				continue;
			}
			UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(new AbilityData(item.Ability, actualCaster), GetTargetByIndexSafe(targets, item.TargetIndex))
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
			if (executionProcess == null)
			{
				executionProcess = ((UnitUseAbility)cmdHandle.Cmd)?.ExecutionProcess;
			}
			yield return null;
			while (!commands.Empty || (executionProcess != null && !executionProcess.IsEnded) || actualCaster.Parts.GetOptional<UnitPartJump>() != null)
			{
				yield return null;
			}
		}
		yield return null;
	}

	private static TargetWrapper GetTargetByIndexSafe(IReadOnlyList<TargetWrapper> targets, int index)
	{
		return targets[Mathf.Clamp(index, 0, targets.Count - 1)];
	}

	private BaseUnitEntity GetDelegateUnit(MechanicEntity caster, CasterType casterType)
	{
		if (casterType == CasterType.PetOfCaster)
		{
			return caster.GetOptional<UnitPartPetOwner>()?.PetUnit ?? (caster as BaseUnitEntity);
		}
		return caster as BaseUnitEntity;
	}

	public IEnumerable<AbilityData> TooltipMultipleTargets(AbilityData rootAbility)
	{
		if (m_UseSelfAsFirstTarget)
		{
			yield return rootAbility;
		}
		foreach (TargetItem target in m_Targets)
		{
			yield return new AbilityData(target.Ability.Get(), GetDelegateUnit(rootAbility.Caster, target.Caster));
		}
	}
}
