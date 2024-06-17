using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/UnitIsInFogOfWar")]
[AllowMultipleComponents]
[TypeId("e071f43640e6239498e7fbf44628a94b")]
public class UnitIsInFogOfWar : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	protected override string GetConditionCaption()
	{
		return $"({Target}) is in fog of war";
	}

	protected override bool CheckCondition()
	{
		return Target.GetValue().IsInFogOfWar;
	}
}
