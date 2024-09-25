using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IQuestObjectiveHandler : ISubscriber
{
	void HandleQuestObjectiveStarted(QuestObjective objective, bool silentStart = false);

	void HandleQuestObjectiveBecameVisible(QuestObjective objective, bool silentStart = false);

	void HandleQuestObjectiveCompleted(QuestObjective objective);

	void HandleQuestObjectiveFailed(QuestObjective objective);
}
