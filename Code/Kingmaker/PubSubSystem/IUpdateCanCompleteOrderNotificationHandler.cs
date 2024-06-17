using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUpdateCanCompleteOrderNotificationHandler : ISubscriber
{
	void HandleUpdateCanCompleteOrderNotificationInJournal();
}
