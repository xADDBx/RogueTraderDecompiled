using Kingmaker.GameModes;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;

public class PartyGainExperienceLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventPartyGainExperience>
{
	public void HandleEvent(GameLogEventPartyGainExperience evt)
	{
		if (!(Game.Instance.CurrentMode != GameModeType.SpaceCombat) || !evt.IsExperienceForDeath)
		{
			GameLogContext.Count = evt.Experience * Game.Instance.Player.ExperienceRatePercent / 100;
			AddMessage(new CombatLogMessage(LogThreadBase.Strings.XpGain.CreateCombatLogMessage(), null, hasTooltip: false));
		}
	}
}
