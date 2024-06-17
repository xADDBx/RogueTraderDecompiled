namespace Kingmaker.Blueprints.Quests;

public interface IQuestObjectiveReference
{
	QuestObjectiveReferenceType GetUsagesFor(BlueprintQuestObjective questObj);
}
