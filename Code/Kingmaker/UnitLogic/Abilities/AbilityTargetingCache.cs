using System.Collections.Generic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities;

public class AbilityTargetingCache
{
	private static AbilityTargetingCache s_Instance;

	public bool IsActive;

	private Dictionary<(BlueprintAbility, TargetWrapper, Vector3), AbilityData.UnavailabilityReasonType> cache = new Dictionary<(BlueprintAbility, TargetWrapper, Vector3), AbilityData.UnavailabilityReasonType>();

	public static AbilityTargetingCache Instance
	{
		get
		{
			if (s_Instance == null && Application.isPlaying)
			{
				s_Instance = new AbilityTargetingCache();
			}
			return s_Instance;
		}
	}

	public bool TryGetReason(BlueprintAbility ability, TargetWrapper target, Vector3 casterPosition, out AbilityData.UnavailabilityReasonType reason)
	{
		if (!IsActive)
		{
			reason = AbilityData.UnavailabilityReasonType.None;
			return false;
		}
		return cache.TryGetValue((ability, target, casterPosition), out reason);
	}

	public void AddEntry(BlueprintAbility ability, TargetWrapper target, Vector3 casterPosition, AbilityData.UnavailabilityReasonType reason)
	{
		if (IsActive)
		{
			cache.Add((ability, target, casterPosition), reason);
		}
	}

	public void SetActive()
	{
		cache.Clear();
		IsActive = true;
	}

	public void SetInactive()
	{
		cache.Clear();
		IsActive = false;
	}
}
