using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventWarningNotification : GameLogEvent<GameLogEventWarningNotification>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IWarningNotificationUIHandler, ISubscriber
	{
		public void HandleWarning(WarningNotificationType warningType, bool addToLog = true, WarningNotificationFormat warningFormat = WarningNotificationFormat.Common, bool withSound = true)
		{
			if (addToLog)
			{
				AddEvent(new GameLogEventWarningNotification(warningType, null));
			}
		}

		public void HandleWarning(string text, bool addToLog = true, WarningNotificationFormat warningFormat = WarningNotificationFormat.Common, bool withSound = true)
		{
			if (addToLog)
			{
				AddEvent(new GameLogEventWarningNotification(WarningNotificationType.None, text));
			}
		}
	}

	public readonly WarningNotificationType Type;

	public readonly string Text;

	public GameLogEventWarningNotification(WarningNotificationType type, string text)
	{
		Type = type;
		Text = text;
	}
}
