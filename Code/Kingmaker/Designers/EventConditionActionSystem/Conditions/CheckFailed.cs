using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("f40da3fe595e435780ba1f1b01dee3a6")]
public class CheckFailed : Condition
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Check")]
	private BlueprintCheckReference m_Check;

	public BlueprintCheck CheckBlueprint => m_Check?.Get();

	protected override string GetConditionCaption()
	{
		return $"Check Failed ({CheckBlueprint})";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.DialogController.LocalFailedChecks.Contains(CheckBlueprint);
	}
}
