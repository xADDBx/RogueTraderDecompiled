using Kingmaker.Localization;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class ColonyStatChangeLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventColonyStatChange>
{
	public void HandleEvent(GameLogEventColonyStatChange evt)
	{
		LocalizedString localizedString = evt.Colony?.Blueprint?.Name;
		GameLogContext.Text = ((localizedString != null) ? ((string)localizedString) : string.Empty);
		GameLogContext.Count = evt.Modifier;
		if (evt.Stat == GameLogEventColonyStatChange.ColonyStatEnum.Contentment && evt.Colony != null)
		{
			AddMessage(LogThreadBase.Strings.ContentmentChangedInColony.CreateCombatLogMessage());
		}
		if (evt.Stat == GameLogEventColonyStatChange.ColonyStatEnum.Contentment && evt.Colony == null)
		{
			AddMessage(LogThreadBase.Strings.ContentmentChangedInAllColonies.CreateCombatLogMessage());
		}
		if (evt.Stat == GameLogEventColonyStatChange.ColonyStatEnum.Efficiency && evt.Colony != null)
		{
			AddMessage(LogThreadBase.Strings.EfficiencyChangedInColony.CreateCombatLogMessage());
		}
		if (evt.Stat == GameLogEventColonyStatChange.ColonyStatEnum.Efficiency && evt.Colony == null)
		{
			AddMessage(LogThreadBase.Strings.EfficiencyChangedInAllColonies.CreateCombatLogMessage());
		}
		if (evt.Stat == GameLogEventColonyStatChange.ColonyStatEnum.Security && evt.Colony != null)
		{
			AddMessage(LogThreadBase.Strings.SecurityChangedInColony.CreateCombatLogMessage());
		}
		if (evt.Stat == GameLogEventColonyStatChange.ColonyStatEnum.Security && evt.Colony == null)
		{
			AddMessage(LogThreadBase.Strings.SecurityChangedInAllColonies.CreateCombatLogMessage());
		}
	}
}
