using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.SystemMapNotification;

namespace Kingmaker.PubSubSystem;

public interface IColonyNotificationUIHandler : ISubscriber
{
	void HandleColonyNotification(string colonyName, ColonyNotificationType type);
}
