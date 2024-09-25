using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("e8267dbbe5d9aee4ca1690deea97ef59")]
public class IsUnconscious : Condition
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override string GetConditionCaption()
	{
		return $"{Unit} is unconscious";
	}

	protected override bool CheckCondition()
	{
		AbstractUnitEntity value = Unit.GetValue();
		if (value != null)
		{
			return !value.IsConscious;
		}
		return false;
	}
}
