using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Code.Designers.EventConditionActionSystem.Conditions;

[TypeId("4219677f5e7c45a7bf8c0f0103ef5a23")]
public class ContextConditionIsUnitHealthLessThan : ContextCondition
{
	[Range(0f, 100f)]
	public int PercentValue;

	protected override string GetConditionCaption()
	{
		return $"Is target health less than {PercentValue}%";
	}

	protected override bool CheckCondition()
	{
		PartHealth healthOptional = base.Target.Entity.GetHealthOptional();
		if (healthOptional == null)
		{
			return false;
		}
		return healthOptional.HitPointsLeft * 100 <= healthOptional.MaxHitPoints * PercentValue;
	}
}
