using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IQuestHandler : ISubscriber
{
	void HandleQuestStarted(Quest quest);

	void HandleQuestCompleted(Quest objective);

	void HandleQuestFailed(Quest objective);

	void HandleQuestUpdated(Quest objective);
}
