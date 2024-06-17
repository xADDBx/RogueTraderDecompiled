using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISetQuestViewedHandler : ISubscriber
{
	void HandleSetQuestViewed(Quest quest);
}
