using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public struct AbilityActionCameraSettings
{
	public readonly Transform CasterTransform;

	public readonly Transform TargetTransform;

	public readonly int TriggerActionCameraChance;

	public bool IsValid
	{
		get
		{
			if (CasterTransform != null)
			{
				return TargetTransform != null;
			}
			return false;
		}
	}

	public AbilityActionCameraSettings(Transform caster, Transform target, int triggerChance)
	{
		CasterTransform = caster;
		TargetTransform = target;
		TriggerActionCameraChance = triggerChance;
	}
}
