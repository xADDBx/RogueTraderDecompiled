using Kingmaker.Blueprints;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventDialog : GameLogEvent<GameLogEventDialog>
{
	public enum EventType
	{
		Start,
		End
	}

	private class EventsHandler : GameLogController.GameEventsHandler, IDialogInteractionHandler, ISubscriber
	{
		public void StartDialogInteraction(BlueprintDialog dialog)
		{
			if (!Game.Instance.CurrentlyLoadedArea.IsPartyArea)
			{
				return;
			}
			BlueprintDialog blueprintDialog = SimpleBlueprintExtendAsObject.Or(dialog, null);
			if (blueprintDialog != null)
			{
				_ = blueprintDialog.Type;
				if (0 == 0 && dialog.Type != DialogType.Book)
				{
					AddEvent(new GameLogEventDialog(EventType.Start, dialog.Type));
				}
			}
		}

		public void StopDialogInteraction(BlueprintDialog dialog)
		{
			if (!Game.Instance.CurrentlyLoadedArea.IsPartyArea)
			{
				return;
			}
			BlueprintDialog blueprintDialog = SimpleBlueprintExtendAsObject.Or(dialog, null);
			if (blueprintDialog != null)
			{
				_ = blueprintDialog.Type;
				if (0 == 0 && dialog.Type != DialogType.Book)
				{
					AddEvent(new GameLogEventDialog(EventType.End, dialog.Type));
				}
			}
		}
	}

	public readonly EventType Event;

	public readonly DialogType DialogType;

	public GameLogEventDialog(EventType @event, DialogType dialogType)
	{
		Event = @event;
		DialogType = dialogType;
	}
}
