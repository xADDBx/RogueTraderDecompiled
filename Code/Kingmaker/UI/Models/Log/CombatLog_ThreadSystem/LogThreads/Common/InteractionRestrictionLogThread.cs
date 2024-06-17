using System;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class InteractionRestrictionLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventInteractionRestriction>
{
	public void HandleEvent(GameLogEventInteractionRestriction evt)
	{
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.Actor;
		switch (evt.Result)
		{
		case GameLogEventInteractionRestriction.ResultType.MissingSkill:
			AddMessage(LogThreadBase.Strings.ThieveryMissing.CreateCombatLogMessage());
			break;
		case GameLogEventInteractionRestriction.ResultType.Jammed:
			AddMessage(LogThreadBase.Strings.LockIsJammed.CreateCombatLogMessage());
			break;
		case GameLogEventInteractionRestriction.ResultType.CantDisarm:
			AddMessage(LogThreadBase.Strings.CantDisarmTrap.CreateCombatLogMessage());
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
