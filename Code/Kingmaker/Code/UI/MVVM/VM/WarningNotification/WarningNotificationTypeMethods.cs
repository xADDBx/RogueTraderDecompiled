namespace Kingmaker.Code.UI.MVVM.VM.WarningNotification;

internal static class WarningNotificationTypeMethods
{
	public static int GetPriority(this WarningNotificationType notificationType)
	{
		switch (notificationType)
		{
		default:
			return 0;
		case WarningNotificationType.GameLoaded:
		case WarningNotificationType.GameSaved:
		case WarningNotificationType.GameSavedAuto:
			return 1;
		case WarningNotificationType.GameSavedQuick:
			return 2;
		case WarningNotificationType.DifficultyChanged:
			return 3;
		case WarningNotificationType.GameSavedInProgress:
			return 9;
		}
	}
}
