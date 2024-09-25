using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;

public class PartyCombatLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventPartyCombatState>, IGameLogEventHandler<GameLogEventPartyGainExperience>, IGameLogEventHandler<GameLogEventUnitGainExperience>
{
	private int m_Experience;

	public void HandleEvent(GameLogEventPartyCombatState evt)
	{
		if (evt.InCombat)
		{
			m_Experience = 0;
			AddMessage(new CombatLogMessage(null, isSeparator: true, GameLogEventAddSeparator.States.Start));
			AddMessage(new CombatLogMessage(null, isSeparator: true, GameLogEventAddSeparator.States.Break));
		}
		AddMessage(evt.InCombat ? new CombatLogMessage(LogThreadBase.Strings.CombatStarted.CreateCombatLogMessage(), null, hasTooltip: false) : new CombatLogMessage(LogThreadBase.Strings.CombatEnded.CreateCombatLogMessage(), null, hasTooltip: false));
		if (!evt.InCombat)
		{
			GameLogContext.Count = m_Experience;
			AddMessage(new CombatLogMessage(LogThreadBase.Strings.XpGain.CreateCombatLogMessage(), null, hasTooltip: false));
			m_Experience = 0;
			AddMessage(new CombatLogMessage(null, isSeparator: true, GameLogEventAddSeparator.States.Finish));
		}
	}

	public void HandleEvent(GameLogEventPartyGainExperience evt)
	{
		m_Experience += evt.Experience * Game.Instance.Player.ExperienceRatePercent / 100;
	}

	public void HandleEvent(GameLogEventUnitGainExperience evt)
	{
	}
}
