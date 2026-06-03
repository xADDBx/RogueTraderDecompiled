using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IQuestObjectiveHandler : ISubscriber
{
	void HandleQuestObjectiveStarted(QuestBookEntityEntry objective, bool silentStart = false);

	void HandleQuestObjectiveBecameVisible(QuestBookEntityEntry objective, bool silentStart = false);

	void HandleQuestObjectiveCompleted(QuestBookEntityEntry objective);

	void HandleQuestObjectiveFailed(QuestBookEntityEntry objective);
}
