using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventColonyProject : GameLogEvent<GameLogEventColonyProject>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IColonizationProjectsHandler, ISubscriber
	{
		private void AddEvent(Colony colony, ColonyProject project, bool started)
		{
			AddEvent(new GameLogEventColonyProject(colony, project, started));
		}

		public void HandleColonyProjectStarted(Colony colony, ColonyProject project)
		{
			AddEvent(colony, project, started: true);
		}

		public void HandleColonyProjectFinished(Colony colony, ColonyProject project)
		{
			AddEvent(colony, project, started: false);
		}
	}

	public readonly Colony Colony;

	public readonly ColonyProject Project;

	public bool Started;

	public GameLogEventColonyProject(Colony colony, ColonyProject project, bool started)
	{
		Colony = colony;
		Project = project;
		Started = started;
	}
}
