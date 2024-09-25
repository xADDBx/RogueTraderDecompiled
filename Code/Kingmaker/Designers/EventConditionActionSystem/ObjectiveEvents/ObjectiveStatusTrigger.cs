using System.Text;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.ElementsSystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.ObjectiveEvents;

[ComponentName("Events/ObjectiveStatusTrigger")]
[AllowMultipleComponents]
[TypeId("0f213d9babee7834da8ccf71a034ad1c")]
public class ObjectiveStatusTrigger : QuestObjectiveComponentDelegate, IQuestObjectiveLogic, IHashable
{
	public QuestObjectiveState objectiveState;

	public ConditionsChecker Conditions;

	public ActionList Actions;

	void IQuestObjectiveLogic.OnStarted()
	{
		if (objectiveState == QuestObjectiveState.Started && Conditions.Check())
		{
			Actions.Run();
		}
	}

	void IQuestObjectiveLogic.OnCompleted()
	{
		if (objectiveState == QuestObjectiveState.Completed && Conditions.Check())
		{
			Actions.Run();
		}
	}

	void IQuestObjectiveLogic.OnFailed()
	{
		if (objectiveState == QuestObjectiveState.Failed && Conditions.Check())
		{
			Actions.Run();
		}
	}

	void IQuestObjectiveLogic.OnBecameVisible()
	{
	}

	public override string GetDescription()
	{
		StringBuilder stringBuilder = new StringBuilder($"When {objectiveState}");
		if (Conditions.HasConditions)
		{
			stringBuilder.Append("\n");
			stringBuilder.Append(ElementsDescription.Conditions(extended: true, Conditions));
		}
		stringBuilder.Append("\n");
		stringBuilder.Append(ElementsDescription.Actions(true, Actions));
		return stringBuilder.ToString();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
