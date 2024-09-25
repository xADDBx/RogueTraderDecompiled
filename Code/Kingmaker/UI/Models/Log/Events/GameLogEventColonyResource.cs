using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventColonyResource : GameLogEvent<GameLogEventColonyResource>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IColonizationResourcesHandler, ISubscriber
	{
		private void AddEvent(BlueprintResource resource, int count)
		{
			AddEvent(new GameLogEventColonyResource(resource, count));
		}

		public void HandleColonyResourcesUpdated(BlueprintResource resource, int count)
		{
			AddEvent(resource, count);
		}

		public void HandleNotFromColonyResourcesUpdated(BlueprintResource resource, int count)
		{
		}
	}

	public readonly BlueprintResource Resource;

	public readonly int Count;

	public GameLogEventColonyResource(BlueprintResource resource, int count)
	{
		Resource = resource;
		Count = count;
	}
}
