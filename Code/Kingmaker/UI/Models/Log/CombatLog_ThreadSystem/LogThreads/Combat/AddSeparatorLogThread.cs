using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class AddSeparatorLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventAddSeparator>
{
	public void HandleEvent(GameLogEventAddSeparator evt)
	{
		if (evt.State == GameLogEventAddSeparator.States.Start)
		{
			CombatLogMessage newMessage = LogThreadBase.Strings.SeparatorStart.CreateCombatLogMessage(isSeparator: true, evt.State);
			AddMessage(newMessage);
		}
		if (evt.State == GameLogEventAddSeparator.States.Finish)
		{
			CombatLogMessage newMessage2 = LogThreadBase.Strings.SeparatorFinish.CreateCombatLogMessage(isSeparator: true, evt.State);
			AddMessage(newMessage2);
		}
	}
}
