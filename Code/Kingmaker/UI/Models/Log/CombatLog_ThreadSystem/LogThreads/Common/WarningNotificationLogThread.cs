using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.UI.Models.Log.Enums;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class WarningNotificationLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventWarningNotification>
{
	public void HandleEvent(GameLogEventWarningNotification evt)
	{
		if (evt.Type == WarningNotificationType.None)
		{
			AddMessage(new CombatLogMessage(evt.Text, LogThreadBase.Colors.WarningLogColor, PrefixIcon.None));
			return;
		}
		string text = LocalizedTexts.Instance.WarningNotification.GetText(evt.Type);
		AddMessage(new CombatLogMessage(text, LogThreadBase.Colors.WarningLogColor, PrefixIcon.None));
	}
}
