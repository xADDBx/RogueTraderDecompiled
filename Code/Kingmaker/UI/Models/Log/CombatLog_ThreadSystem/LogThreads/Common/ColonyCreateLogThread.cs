using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class ColonyCreateLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventColonyCreate>
{
	public void HandleEvent(GameLogEventColonyCreate evt)
	{
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.Planet;
		GameLogContext.Text = evt.Colony.Blueprint.ColonyName;
		GameLogContext.SecondText = evt.Planet.Blueprint.Name;
		AddMessage(LogThreadBase.Strings.CreateColony.CreateCombatLogMessage());
	}
}
