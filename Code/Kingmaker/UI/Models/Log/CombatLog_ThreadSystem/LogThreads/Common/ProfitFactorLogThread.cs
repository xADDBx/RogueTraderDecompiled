using System;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class ProfitFactorLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventProfitFactor>
{
	public void HandleEvent(GameLogEventProfitFactor evt)
	{
		GameLogContext.Text = Math.Abs(evt.Value).ToString();
		AddMessage((evt.Value < 0f) ? LogThreadBase.Strings.ProfitFactorLost.CreateCombatLogMessage() : LogThreadBase.Strings.ProfitFactorGained.CreateCombatLogMessage());
	}
}
