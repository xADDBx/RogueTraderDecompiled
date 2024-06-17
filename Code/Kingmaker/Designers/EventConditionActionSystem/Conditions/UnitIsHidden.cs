using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("d47f198683f34b0f8e13012d2a635017")]
public class UnitIsHidden : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override string GetConditionCaption()
	{
		return $"({Unit}) is hidden";
	}

	protected override bool CheckCondition()
	{
		return !Unit.GetValue().IsInGame;
	}
}
