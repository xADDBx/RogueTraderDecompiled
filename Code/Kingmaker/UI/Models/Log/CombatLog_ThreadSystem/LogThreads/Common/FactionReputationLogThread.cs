using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class FactionReputationLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventFactionReputation>
{
	public void HandleEvent(GameLogEventFactionReputation evt)
	{
		GameLogContext.Text = UIStrings.Instance.CharacterSheet.GetFactionLabel(evt.FactionType);
		GameLogContext.Count = Math.Abs(evt.Points);
		AddMessage(new CombatLogMessage((evt.Points < 0) ? LogThreadBase.Strings.FactionReputationLost.CreateCombatLogMessage() : LogThreadBase.Strings.FactionReputationGained.CreateCombatLogMessage(), null, hasTooltip: false));
	}
}
