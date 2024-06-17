using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Designers.Quests.Common;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/SetObjectiveStatus")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("31abd123cda0b4147b509219dbb097a0")]
public class SetObjectiveStatus : GameAction, IQuestObjectiveReference
{
	public SummonPoolCountTrigger.ObjectiveStatus Status;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Objective")]
	private BlueprintQuestObjectiveReference m_Objective;

	public bool StartObjectiveIfNone;

	public BlueprintQuestObjective Objective => m_Objective?.Get();

	public override void RunAction()
	{
		if (StartObjectiveIfNone && GameHelper.Quests.GetObjectiveState(Objective) == QuestObjectiveState.None)
		{
			GameHelper.Quests.GiveObjective(Objective);
		}
		if (GameHelper.Quests.GetObjectiveState(Objective) != 0)
		{
			switch (Status)
			{
			case SummonPoolCountTrigger.ObjectiveStatus.Complete:
				GameHelper.Quests.CompleteObjective(Objective);
				break;
			case SummonPoolCountTrigger.ObjectiveStatus.Fail:
				GameHelper.Quests.FailObjective(Objective);
				break;
			}
		}
	}

	public override string GetCaption()
	{
		return $"Set Objective Status ({Status} to {Objective})";
	}

	public QuestObjectiveReferenceType GetUsagesFor(BlueprintQuestObjective questObj)
	{
		if (questObj != Objective)
		{
			return QuestObjectiveReferenceType.None;
		}
		if (Status != 0)
		{
			return QuestObjectiveReferenceType.Give | QuestObjectiveReferenceType.Fail;
		}
		return QuestObjectiveReferenceType.Give | QuestObjectiveReferenceType.Complete;
	}
}
