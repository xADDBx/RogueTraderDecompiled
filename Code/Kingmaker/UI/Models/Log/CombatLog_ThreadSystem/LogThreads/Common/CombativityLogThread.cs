using System;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class CombativityLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventCombativity>
{
	public void HandleEvent(GameLogEventCombativity evt)
	{
		GameLogContext.Text = Math.Abs(evt.Value).ToString();
		AddMessage((evt.Value < 0f) ? LogThreadBase.Strings.CombativityLost.CreateCombatLogMessage() : LogThreadBase.Strings.CombativityGained.CreateCombatLogMessage());
	}
}
