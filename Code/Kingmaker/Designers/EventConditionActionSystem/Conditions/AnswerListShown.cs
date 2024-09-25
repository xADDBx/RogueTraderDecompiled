using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("1d053178c659ba2469e4158f75e135e0")]
public class AnswerListShown : Condition
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("AnswersList")]
	private BlueprintAnswersListReference m_AnswersList;

	[Tooltip("Only check shown answer lists in current dialog instance. By default whole game history matters.")]
	public bool CurrentDialog;

	public BlueprintAnswersList AnswersList => m_AnswersList?.Get();

	protected override string GetConditionCaption()
	{
		return $"Answer List Shown ({AnswersList})";
	}

	protected override bool CheckCondition()
	{
		if (!CurrentDialog)
		{
			return Game.Instance.Player.Dialog.ShownAnswerLists.Contains(AnswersList);
		}
		return Game.Instance.DialogController.LocalShownAnswerLists.Contains(AnswersList);
	}
}
