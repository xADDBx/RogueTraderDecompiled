using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/QuestStatus")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("49ab5e967cc13354493c3f9395e55611")]
public class QuestStatus : Condition, IQuestReference
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Quest")]
	private BlueprintQuestReference m_Quest;

	public QuestState State;

	public BlueprintQuest Quest => m_Quest?.Get();

	protected override string GetConditionCaption()
	{
		return $"Quest ({Quest} is {State})";
	}

	protected override bool CheckCondition()
	{
		QuestState questState = GameHelper.Quests.GetQuestState(Quest);
		return State == questState;
	}

	public QuestReferenceType GetUsagesFor(BlueprintQuest quest)
	{
		if (quest != Quest)
		{
			return QuestReferenceType.None;
		}
		return QuestReferenceType.CheckStatus;
	}
}
