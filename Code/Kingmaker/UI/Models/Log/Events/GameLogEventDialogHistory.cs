using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventDialogHistory : GameLogEvent<GameLogEventDialogHistory>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IDialogHistoryHandler, ISubscriber
	{
		public void HandleOnDialogHistory(IDialogShowData showData)
		{
			if (Game.Instance.DialogController.Dialog.Type != DialogType.Book)
			{
				AddEvent(new GameLogEventDialogHistory(showData));
			}
		}
	}

	private readonly IDialogShowData m_ShowData;

	public GameLogEventDialogHistory(IDialogShowData showData)
	{
		m_ShowData = showData;
	}

	public string GetText(DialogColors dialogColors)
	{
		return m_ShowData.GetText(dialogColors);
	}
}
