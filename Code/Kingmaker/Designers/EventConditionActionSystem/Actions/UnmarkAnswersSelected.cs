using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("5082ab8661a74b97891c7d0812fc0c4c")]
public class UnmarkAnswersSelected : GameAction
{
	[ValidateNoNullEntries]
	[SerializeField]
	[FormerlySerializedAs("Answers")]
	private BlueprintAnswerReference[] m_Answers;

	public ReferenceArrayProxy<BlueprintAnswer> Answers
	{
		get
		{
			BlueprintReference<BlueprintAnswer>[] answers = m_Answers;
			return answers;
		}
	}

	public override string GetCaption()
	{
		string text = "";
		for (int i = 0; i < Answers.Length; i++)
		{
			if (i > 0)
			{
				text += ", ";
			}
			text += Answers[i].ToString();
		}
		return "Unmark Answers Selected (" + text + ")";
	}

	protected override void RunAction()
	{
		foreach (BlueprintAnswer answer in Answers)
		{
			Game.Instance.Player.Dialog.SelectedAnswersRemove(answer);
		}
	}
}
