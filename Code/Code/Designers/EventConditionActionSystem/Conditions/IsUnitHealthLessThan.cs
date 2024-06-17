using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Code.Designers.EventConditionActionSystem.Conditions;

[TypeId("9dc845daf78a4591a021c79e84825ed5")]
public class IsUnitHealthLessThan : Condition
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[Range(0f, 100f)]
	public int PercentValue;

	protected override string GetConditionCaption()
	{
		return $"Is {Unit} health less than {PercentValue}%";
	}

	protected override bool CheckCondition()
	{
		PartHealth healthOptional = Unit.GetValue().GetHealthOptional();
		if (healthOptional == null)
		{
			return false;
		}
		float num = (float)healthOptional.HitPointsLeft / (float)healthOptional.MaxHitPoints;
		float num2 = (float)PercentValue / 100f;
		if (!(Mathf.Abs(num2 - num) < float.Epsilon))
		{
			return num < num2;
		}
		return false;
	}
}
