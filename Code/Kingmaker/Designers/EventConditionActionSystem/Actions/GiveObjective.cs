using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/GiveObjective")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("3af21dd61b640c941b5704f3df91e16d")]
public class GiveObjective : GameAction, IQuestObjectiveReference
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Objective")]
	private BlueprintQuestObjectiveReference m_Objective;

	public BlueprintQuestObjective Objective => m_Objective?.Get();

	public override string GetDescription()
	{
		return $"Выдает игроку обжектив или аддендум {Objective}.\n" + "Выдаются только обжективы и аддендумы в статусе None. Им выставляется статуст Started";
	}

	public override void RunAction()
	{
		GameHelper.Quests.GiveObjective(Objective);
	}

	public override string GetCaption()
	{
		return $"Give Objective ({Objective})";
	}

	public QuestObjectiveReferenceType GetUsagesFor(BlueprintQuestObjective questObj)
	{
		if (questObj != Objective)
		{
			return QuestObjectiveReferenceType.None;
		}
		return QuestObjectiveReferenceType.Give;
	}
}
