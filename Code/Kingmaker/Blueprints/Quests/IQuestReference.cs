namespace Kingmaker.Blueprints.Quests;

public interface IQuestReference
{
	QuestReferenceType GetUsagesFor(BlueprintQuest quest);
}
