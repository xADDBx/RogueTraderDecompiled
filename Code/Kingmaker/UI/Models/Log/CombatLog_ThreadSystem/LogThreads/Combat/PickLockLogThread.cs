using System;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class PickLockLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventPickLock>
{
	public void HandleEvent(GameLogEventPickLock evt)
	{
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.Actor;
		switch (evt.Result)
		{
		case GameLogEventPickLock.ResultType.Success:
			AddMessage(LogThreadBase.Strings.PickLockSuccess.CreateCombatLogMessage());
			break;
		case GameLogEventPickLock.ResultType.Fail:
			AddMessage(LogThreadBase.Strings.PickLockFail.CreateCombatLogMessage());
			break;
		case GameLogEventPickLock.ResultType.CriticalFail:
			AddMessage(LogThreadBase.Strings.PickLockCriticalFail.CreateCombatLogMessage());
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
