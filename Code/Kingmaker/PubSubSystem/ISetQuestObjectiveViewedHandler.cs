using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISetQuestObjectiveViewedHandler : ISubscriber
{
	void HandleSetQuestObjectiveViewed(QuestObjective objective);
}
