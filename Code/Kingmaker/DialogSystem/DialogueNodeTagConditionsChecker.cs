using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.DialogSystem;

[AllowedOn(typeof(BlueprintCue))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("3be4f082fe2140879ce3483580474724")]
public class DialogueNodeTagConditionsChecker : BlueprintComponent
{
	[SerializeField]
	private ConditionsChecker _conditions;

	public bool CheckConditions()
	{
		return _conditions.Check();
	}
}
