using System;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class NavigatorResourceLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventNavigatorResource>
{
	public void HandleEvent(GameLogEventNavigatorResource evt)
	{
		GameLogContext.Text = Math.Abs(evt.Value).ToString();
		AddMessage((evt.Value < 0f) ? LogThreadBase.Strings.NavigatorResourceLost.CreateCombatLogMessage() : LogThreadBase.Strings.NavigatorResourceGained.CreateCombatLogMessage());
	}
}
