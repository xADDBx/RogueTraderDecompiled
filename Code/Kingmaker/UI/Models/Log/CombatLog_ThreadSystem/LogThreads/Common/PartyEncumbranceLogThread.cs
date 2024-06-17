using System;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UnitLogic;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class PartyEncumbranceLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventPartyEncumbranceChanged>
{
	public void HandleEvent(GameLogEventPartyEncumbranceChanged evt)
	{
		switch (evt.CurrentEncumbrance)
		{
		case Encumbrance.Light:
			AddMessage(LogThreadBase.Strings.PartyEncumbranceLight.CreateCombatLogMessage());
			break;
		case Encumbrance.Medium:
			AddMessage(LogThreadBase.Strings.PartyEncumbranceMedium.CreateCombatLogMessage());
			break;
		case Encumbrance.Heavy:
			AddMessage(LogThreadBase.Strings.PartyEncumbranceHeavy.CreateCombatLogMessage());
			break;
		case Encumbrance.Overload:
			AddMessage(LogThreadBase.Strings.PartyEncumbranceOverload.CreateCombatLogMessage());
			break;
		default:
			throw new ArgumentOutOfRangeException("CurrentEncumbrance", evt.CurrentEncumbrance, "CurrentEncumbrance is out of range");
		}
	}
}
