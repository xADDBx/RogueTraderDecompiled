using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDialogNotificationHandler : ISubscriber
{
	void AddCustomNotification(string line);
}
