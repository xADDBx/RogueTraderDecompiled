using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/OrAndLogic")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("1d392c8d9feed78408fbcb18f9468fb9")]
public class OrAndLogic : Condition
{
	[TextArea]
	public string Comment;

	public ConditionsChecker ConditionsChecker;

	protected override string GetConditionCaption()
	{
		return ConditionsChecker.Operation.ToString() + " (" + ConditionsChecker.Conditions.Length + ")(" + Comment + ")";
	}

	protected override bool CheckCondition()
	{
		return ConditionsChecker.Check();
	}
}
