using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace Kingmaker.Code.Mechanics.Adapters;

[Serializable]
[TypeId("8fe1d89d3de445bd84f475e9b84978a6")]
public class ContextAbilityNthTargetPointEvaluator : PositionEvaluator
{
	[SerializeField]
	private int m_TargetIndex;

	public override string GetCaption()
	{
		return $"Target #{m_TargetIndex} position from Ability Execution context";
	}

	protected override Vector3 GetValueInternal()
	{
		if (m_TargetIndex < 0 || !(ContextData<MechanicsContext.Data>.Current?.Context is AbilityExecutionContext { AllTargets: not null } abilityExecutionContext) || m_TargetIndex >= abilityExecutionContext.AllTargets.Count)
		{
			throw new FailToEvaluateException(this);
		}
		return abilityExecutionContext.AllTargets[m_TargetIndex].Point;
	}
}
