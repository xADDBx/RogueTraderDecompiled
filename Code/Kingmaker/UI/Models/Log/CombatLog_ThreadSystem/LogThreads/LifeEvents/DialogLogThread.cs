using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;

public class DialogLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventDialog>
{
	public void HandleEvent(GameLogEventDialog evt)
	{
		CombatLogMessage combatLogMessage = null;
		switch (evt.DialogType)
		{
		case DialogType.Common:
			combatLogMessage = ((evt.Event == GameLogEventDialog.EventType.Start) ? LogThreadBase.Strings.DialogStarted.CreateCombatLogMessage() : LogThreadBase.Strings.DialogEnded.CreateCombatLogMessage());
			break;
		case DialogType.Book:
			combatLogMessage = ((evt.Event == GameLogEventDialog.EventType.Start) ? LogThreadBase.Strings.BookStarted.CreateCombatLogMessage() : LogThreadBase.Strings.BookEnded.CreateCombatLogMessage());
			break;
		case DialogType.StarSystemEvent:
			combatLogMessage = ((evt.Event == GameLogEventDialog.EventType.Start) ? LogThreadBase.Strings.DialogStarted.CreateCombatLogMessage() : LogThreadBase.Strings.DialogEnded.CreateCombatLogMessage());
			break;
		default:
			PFLog.UI.Error("Trying to finish dialogue of invalid type {0}", evt.DialogType);
			break;
		case DialogType.Epilog:
			break;
		}
		if (combatLogMessage != null)
		{
			AddMessage(new CombatLogMessage(combatLogMessage, null, hasTooltip: false));
		}
	}
}
