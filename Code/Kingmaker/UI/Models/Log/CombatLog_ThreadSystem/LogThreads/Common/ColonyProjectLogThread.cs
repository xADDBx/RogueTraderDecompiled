using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class ColonyProjectLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventColonyProject>
{
	public void HandleEvent(GameLogEventColonyProject evt)
	{
		GameLogContext.Text = evt.Colony.Blueprint.ColonyName;
		GameLogContext.SecondText = evt.Project.Blueprint.Name;
		if (evt.Started)
		{
			AddMessage(LogThreadBase.Strings.ProjectStarted.CreateCombatLogMessage());
		}
		else
		{
			AddMessage(LogThreadBase.Strings.ProjectFinished.CreateCombatLogMessage());
		}
	}
}
