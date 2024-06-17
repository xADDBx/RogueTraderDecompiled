using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IWarningNotificationUIHandler : ISubscriber
{
	void HandleWarning(WarningNotificationType warningType, bool addToLog = true, WarningNotificationFormat warningFormat = WarningNotificationFormat.Common, bool withSound = true);

	void HandleWarning(string text, bool addToLog = true, WarningNotificationFormat warningFormat = WarningNotificationFormat.Common, bool withSound = true);
}
