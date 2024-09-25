using System;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;

public class ScrapCollectionLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventScrapCollection>
{
	public void HandleEvent(GameLogEventScrapCollection evt)
	{
		int count = evt.Count;
		GameLogContext.Tooltip = count;
		GameLogContext.Count = count;
		switch (evt.Event)
		{
		case GameLogEventScrapCollection.EventType.ScrapGained:
		{
			GameLogMessage scrapGained = LogThreadBase.Strings.ScrapGained;
			AddMessage(new CombatLogMessage(scrapGained.CreateCombatLogMessage(), null, hasTooltip: false));
			break;
		}
		case GameLogEventScrapCollection.EventType.ScrapSpend:
		{
			GameLogMessage scrapSpend = LogThreadBase.Strings.ScrapSpend;
			AddMessage(new CombatLogMessage(scrapSpend.CreateCombatLogMessage(), null, hasTooltip: false));
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
