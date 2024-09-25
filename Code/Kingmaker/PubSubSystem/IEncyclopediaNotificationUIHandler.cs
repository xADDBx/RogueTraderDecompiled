using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IEncyclopediaNotificationUIHandler : ISubscriber
{
	void HandleEncyclopediaNotification(string link, string encyclopediaName);
}
