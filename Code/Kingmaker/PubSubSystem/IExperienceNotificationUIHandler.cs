using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IExperienceNotificationUIHandler : ISubscriber
{
	void HandleExperienceNotification(int amount);
}
