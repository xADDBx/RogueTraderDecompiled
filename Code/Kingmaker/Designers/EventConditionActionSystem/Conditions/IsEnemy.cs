using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("9ebbae81b66bb174b9050f3a92549ca3")]
public class IsEnemy : Condition
{
	[SerializeReference]
	public AbstractUnitEvaluator FirstUnit;

	[SerializeReference]
	public AbstractUnitEvaluator SecondUnit;

	protected override string GetConditionCaption()
	{
		return $"{FirstUnit} is enemy {SecondUnit}";
	}

	protected override bool CheckCondition()
	{
		return FirstUnit.GetValue().IsEnemy(SecondUnit.GetValue());
	}
}
