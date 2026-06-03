using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISetQuestObjectiveViewedHandler : ISubscriber
{
	void HandleSetQuestObjectiveViewed(QuestBookEntityEntry objective);
}
