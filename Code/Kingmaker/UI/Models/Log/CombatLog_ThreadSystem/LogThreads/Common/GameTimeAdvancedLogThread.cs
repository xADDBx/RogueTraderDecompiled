using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class GameTimeAdvancedLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventTimeAdvanced>
{
	public void HandleEvent(GameLogEventTimeAdvanced evt)
	{
		CombatLogMessage combatLogMessage = BlueprintRoot.Instance.LocalizedTexts.GameLog.TimePassed.CreateCombatLogMessage();
		string compactPeriodString = BlueprintRoot.Instance.Calendar.GetCompactPeriodString(evt.DeltaTime);
		if (!string.IsNullOrEmpty(compactPeriodString))
		{
			string timePassedMessage = ((combatLogMessage != null) ? (combatLogMessage.Message + ": " + compactPeriodString) : compactPeriodString);
			combatLogMessage?.ReplaceMessage(timePassedMessage);
			AddMessage(combatLogMessage);
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(timePassedMessage, addToLog: false);
			});
		}
	}
}
