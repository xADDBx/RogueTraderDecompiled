using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/UnitEqual")]
[AllowMultipleComponents]
[TypeId("8ddc3b555bdf08448b9083dba5153210")]
public class UnitEqual : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator FirstUnit;

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator SecondUnit;

	protected override string GetConditionCaption()
	{
		return $"Units equal ({FirstUnit} == {SecondUnit})";
	}

	protected override bool CheckCondition()
	{
		return FirstUnit.GetValue() == SecondUnit.GetValue();
	}
}
