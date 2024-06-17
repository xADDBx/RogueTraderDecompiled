using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class ColonyChronicleLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventColonyChronicle>
{
	public void HandleEvent(GameLogEventColonyChronicle evt)
	{
		GameLogContext.Text = evt.Colony.Blueprint.ColonyName;
		GameLogContext.SecondText = evt.Chronicle.Blueprint.Name;
		AddMessage(LogThreadBase.Strings.ChronicleFinished.CreateCombatLogMessage());
	}
}
