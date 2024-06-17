using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("d611bb4cba634bc4bb6e5057b07ffc97")]
public class CheckPassed : Condition
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Check")]
	private BlueprintCheckReference m_Check;

	public BlueprintCheck CheckBlueprint => m_Check?.Get();

	protected override string GetConditionCaption()
	{
		return $"Check Passed ({CheckBlueprint})";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.DialogController.LocalPassedChecks.Contains(CheckBlueprint);
	}
}
