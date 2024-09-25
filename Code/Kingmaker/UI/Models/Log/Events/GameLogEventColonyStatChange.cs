using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventColonyStatChange : GameLogEvent<GameLogEventColonyStatChange>
{
	public enum ColonyStatEnum
	{
		Contentment,
		Security,
		Efficiency
	}

	private class EventsHandler : GameLogController.GameEventsHandler, IColonizationEachStatHandler, ISubscriber
	{
		private void AddEvent(Colony colony, ColonyStatEnum stat, int modifier)
		{
			AddEvent(new GameLogEventColonyStatChange(colony, stat, modifier));
		}

		public void HandleContentmentChanged(Colony colony, int modifier)
		{
			AddEvent(colony, ColonyStatEnum.Contentment, modifier);
		}

		public void HandleContentmentInAllColoniesChanged(int modifier)
		{
			AddEvent(null, ColonyStatEnum.Contentment, modifier);
		}

		public void HandleEfficiencyChanged(Colony colony, int modifier)
		{
			AddEvent(colony, ColonyStatEnum.Efficiency, modifier);
		}

		public void HandleEfficiencyInAllColoniesChanged(int modifier)
		{
			AddEvent(null, ColonyStatEnum.Efficiency, modifier);
		}

		public void HandleSecurityChanged(Colony colony, int modifier)
		{
			AddEvent(colony, ColonyStatEnum.Security, modifier);
		}

		public void HandleSecurityInAllColoniesChanged(int modifier)
		{
			AddEvent(null, ColonyStatEnum.Security, modifier);
		}
	}

	public readonly Colony Colony;

	public readonly ColonyStatEnum Stat;

	public readonly int Modifier;

	public GameLogEventColonyStatChange(Colony colony, ColonyStatEnum stat, int modifier)
	{
		Colony = colony;
		Stat = stat;
		Modifier = modifier;
	}
}
