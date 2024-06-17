using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IQuestObjectiveHandler : ISubscriber
{
	void HandleQuestObjectiveStarted(QuestObjective objective);

	void HandleQuestObjectiveBecameVisible(QuestObjective objective);

	void HandleQuestObjectiveCompleted(QuestObjective objective);

	void HandleQuestObjectiveFailed(QuestObjective objective);
}
