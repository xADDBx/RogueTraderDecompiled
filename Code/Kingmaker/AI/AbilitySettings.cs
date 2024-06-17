using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.AI;

[Serializable]
public class AbilitySettings
{
	public AbilitySourceWrapper AbilitySource;

	[Tooltip("Unit will use ability only onto targets satisfied one of these conditions.\nEmpty conditions means everyone meet conditions.")]
	public PropertyCalculator[] ValidTargetConditions;

	[Tooltip("If enabled unit will try to spend a few move points to get better position for cast this ability.")]
	public bool HasMovePointsLimit;

	[ShowIf("HasMovePointsLimit")]
	[Tooltip("Unit will try to spend up to set number of move points to get better position for cast this ability.")]
	public int MovePointsLimit;

	public bool IsAOE;

	[Tooltip("Unit will use ability only if can hit enough number of targets.")]
	[ShowIf("IsAOE")]
	public int MustHitTargetsCount = 1;

	[Tooltip("Target is counted only if it is VALID (!) and is satisfying one of these conditions.\nEmpty conditions means everyone meet conditions.")]
	[ShowIf("IsAOE")]
	public PropertyCalculator[] CountTargetConditions;

	[Tooltip("Unit will try use ability onto target satisfied one of these conditions rather than other targets.")]
	public PropertyCalculator[] HighPriorityTargetConditions;

	[Tooltip("Unit will try use ability onto target satisfied one of these conditions if there are no other targets.")]
	public PropertyCalculator[] LowPriorityTargetConditions;

	[Tooltip("Unit won't cast the ability on game difficulty lower than this one")]
	public GameDifficultyOption MinDifficultySetting;

	[Tooltip("Unit won't cast the ability until Nth round starts.")]
	public int CantCastUntilRound;

	private const int DefaultMustHitTargetsCount = 1;

	public List<BlueprintAbility> Abilities => AbilitySource?.Abilities;

	public bool IsValidTarget(PropertyContext context)
	{
		return CheckAgainstConditions(context, ValidTargetConditions, defaultResult: true);
	}

	public bool IsHighPriority(PropertyContext context)
	{
		return CheckAgainstConditions(context, HighPriorityTargetConditions);
	}

	public bool IsLowPriority(PropertyContext context)
	{
		return CheckAgainstConditions(context, LowPriorityTargetConditions);
	}

	public bool IsTargetCounts(PropertyContext context)
	{
		if (IsValidTarget(context))
		{
			return CheckAgainstConditions(context, CountTargetConditions, defaultResult: true);
		}
		return false;
	}

	private bool CheckAgainstConditions(PropertyContext context, PropertyCalculator[] conditions, bool defaultResult = false)
	{
		if (conditions == null || conditions.Length == 0)
		{
			return defaultResult;
		}
		for (int i = 0; i < conditions.Length; i++)
		{
			if (conditions[i].GetValue(context) > 0)
			{
				return true;
			}
		}
		return false;
	}
}
