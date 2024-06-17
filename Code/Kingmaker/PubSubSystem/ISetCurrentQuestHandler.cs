using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISetCurrentQuestHandler : ISubscriber
{
	void HandleSetCurrentQuest(Quest quest);
}
