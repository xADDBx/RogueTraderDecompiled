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

[ComponentName("Condition/ObjectiveStatus")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("e77a58dedc8350a4faa55f5e66b9f575")]
public class ObjectiveStatus : Condition, IQuestObjectiveReference
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("QuestObjective")]
	private BlueprintQuestObjectiveReference m_QuestObjective;

	public QuestObjectiveState State;

	public BlueprintQuestObjective QuestObjective => m_QuestObjective?.Get();

	protected override string GetConditionCaption()
	{
		return $"Objective ({QuestObjective} is {State})";
	}

	protected override bool CheckCondition()
	{
		return GameHelper.Quests.GetObjectiveState(QuestObjective) == State;
	}

	public QuestObjectiveReferenceType GetUsagesFor(BlueprintQuestObjective questObj)
	{
		if (questObj != QuestObjective)
		{
			return QuestObjectiveReferenceType.None;
		}
		return QuestObjectiveReferenceType.CheckStatus;
	}
}
