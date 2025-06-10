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
[TypeId("def1c2cbb0e01744292ca51e8f2cd326")]
public class MarkAnswersSelected : GameAction
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

	public override string GetDescription()
	{
		return "Помечает указанные ответы игрока в диалоге как выбранные.";
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
		return "Mark Answers Selected (" + text + ")";
	}

	protected override void RunAction()
	{
		foreach (BlueprintAnswer answer in Answers)
		{
			Game.Instance.Player.Dialog.SelectedAnswersAdd(answer);
		}
	}
}
