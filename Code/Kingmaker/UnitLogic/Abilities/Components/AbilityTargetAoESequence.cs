using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.StatefulRandom;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[KDB("Позволяет выбрать последовательно несколько целей и скастовать на них соответствующие внутренние способности")]
[TypeId("3f4d31df59a34d89ac159b5015022298")]
public class AbilityTargetAoESequence : AbilityCustomLogic, IAbilityMultiTarget, IAbilityGetFirstSingleTargetForTooltip, IAbilityGetTooltipTarget
{
	public enum CastPosition
	{
		PreviousCastPosition,
		PreviousTargetPosition
	}

	[Serializable]
	private struct QueuedAbility
	{
		public BlueprintAbilityReference Ability;

		public CastPosition CastPosition;
	}

	[KDB("Первая внутреняя абилка для каста")]
	public BlueprintAbilityReference _firstAbility;

	[KDB("Внутренние абилки, которые будут последовательно запускаться после первой")]
	[SerializeField]
	private List<QueuedAbility> _abilityQueue;

	[KDB("Действия, которые запустятся в конце каста данной родительской абилки, после всех внутренних абилок")]
	[SerializeField]
	private ActionList _cleanupActions;

	public int TargetAbilityCount => _abilityQueue.Count + 1;

	public bool TryGetNextTargetAbility(AbilityData rootAbility, int targetIndex, out AbilityData ability)
	{
		if (targetIndex < 0 || targetIndex >= TargetAbilityCount)
		{
			ability = null;
			return false;
		}
		MechanicEntity caster = rootAbility.Caster;
		Vector3? overrideCastPosition = null;
		BlueprintAbility blueprint;
		if (targetIndex == 0)
		{
			blueprint = _firstAbility.Get();
		}
		else
		{
			blueprint = _abilityQueue[targetIndex - 1].Ability.Get();
			IReadOnlyList<TargetWrapper> targets = Game.Instance.SelectedAbilityHandler?.MultiTargetHandler.Targets;
			overrideCastPosition = GetCastPositionFromTargets(targets, targetIndex, caster);
		}
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			ability = new AbilityData(blueprint, caster)
			{
				OverrideCastPosition = overrideCastPosition
			};
		}
		return true;
	}

	public IEnumerable<AbilityData> TooltipSingleTarget(AbilityData rootAbility)
	{
		if (_firstAbility != null)
		{
			yield return new AbilityData(_firstAbility.Get(), rootAbility.Caster);
		}
		else
		{
			yield return null;
		}
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		List<TargetWrapper> targets = context.AllTargets;
		MechanicEntity caster2 = context.Caster;
		if (!(caster2 is UnitEntity caster))
		{
			PFLog.Default.Error(context.AbilityBlueprint, "Caster unit is missing");
			yield break;
		}
		IEnumerator cast = CastQueuedAbility(_firstAbility.Get(), caster, targets, 0);
		while (cast.MoveNext())
		{
			yield return null;
		}
		for (int i = 0; i < _abilityQueue.Count; i++)
		{
			int castIndex = i + 1;
			cast = CastQueuedAbility(_abilityQueue[i].Ability.Get(), caster, targets, castIndex);
			while (cast.MoveNext())
			{
				yield return null;
			}
		}
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
		using (context.GetDataScope())
		{
			_cleanupActions?.Run();
		}
	}

	private IEnumerator CastQueuedAbility(BlueprintAbility ability, UnitEntity caster, List<TargetWrapper> targets, int castIndex)
	{
		Vector3 castPositionFromTargets = GetCastPositionFromTargets(targets, castIndex, caster);
		RulePerformAbility rulePerformAbility = new RulePerformAbility(new AbilityData(ability, caster)
		{
			OverrideCastPosition = castPositionFromTargets
		}, GetTargetByIndexSafe(targets, castIndex), castPositionFromTargets)
		{
			ForceFreeAction = true
		};
		Rulebook.Trigger(rulePerformAbility);
		AbilityExecutionProcess executionProcess = rulePerformAbility.Result;
		while ((executionProcess != null && !executionProcess.IsEnded) || caster.Parts.GetOptional<UnitPartJump>() != null)
		{
			yield return null;
		}
		yield return null;
	}

	private Vector3 GetCastPositionFromTargets(IReadOnlyList<TargetWrapper> targets, int castIndex, MechanicEntity originalCaster)
	{
		Vector3 position = originalCaster.Position;
		if (targets == null || castIndex <= 0 || castIndex > targets.Count)
		{
			return position;
		}
		for (int num = castIndex; num > 0; num--)
		{
			int index = num - 1;
			if (_abilityQueue[index].CastPosition == CastPosition.PreviousTargetPosition)
			{
				return targets[num - 1].Point;
			}
		}
		return position;
	}

	private static TargetWrapper GetTargetByIndexSafe(IReadOnlyList<TargetWrapper> targets, int index)
	{
		return targets[Mathf.Clamp(index, 0, targets.Count - 1)];
	}
}
