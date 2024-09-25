using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("fe3ffdeec75949159783a8607d95a321")]
public class UnitIsNull : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	protected override string GetConditionCaption()
	{
		return $"({Target}) is NULL";
	}

	protected override bool CheckCondition()
	{
		return !Target.CanEvaluate();
	}
}
