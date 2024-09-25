using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class ColonyResourcesLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventColonyResource>
{
	public void HandleEvent(GameLogEventColonyResource evt)
	{
		GameLogContext.Text = evt.Resource.Name;
		GameLogContext.Count = evt.Count;
		if (evt.Count > 0)
		{
			AddMessage(LogThreadBase.Strings.ResourceGained.CreateCombatLogMessage());
		}
		else
		{
			AddMessage(LogThreadBase.Strings.ResourceLost.CreateCombatLogMessage());
		}
	}
}
