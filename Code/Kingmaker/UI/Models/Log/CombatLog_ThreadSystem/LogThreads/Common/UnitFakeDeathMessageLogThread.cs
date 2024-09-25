using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Enums;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using UnityEngine;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class UnitFakeDeathMessageLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventFakeCompanionDeath>
{
	public void HandleEvent(GameLogEventFakeCompanionDeath evt)
	{
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.Unit;
		AddMessage(new CombatLogMessage(evt.Message, default(Color), PrefixIcon.None));
	}
}
