using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IQuestNotificatoinUIHandler : ISubscriber
{
	void HandleQuestNotificationClick(Quest quest = null);
}
