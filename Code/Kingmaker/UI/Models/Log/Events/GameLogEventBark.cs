using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventBark : GameLogEvent<GameLogEventBark>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IBarkHandler, ISubscriber<IEntity>, ISubscriber
	{
		public void HandleOnShowBark(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				AddEvent(new GameLogEventBark(EventInvokerExtensions.Entity, text));
			}
		}

		public void HandleOnShowBarkWithName(string text, string name, Color nameColor)
		{
			if (!string.IsNullOrEmpty(text))
			{
				AddEvent(new GameLogEventBark(EventInvokerExtensions.Entity, text, name, nameColor));
			}
		}

		public void HandleOnShowLinkedBark(string text, string encyclopediaLink)
		{
		}

		public void HandleOnHideBark()
		{
		}
	}

	public readonly Entity Actor;

	public readonly string Text;

	public readonly string OverrideName;

	public readonly Color OverrideNameColor;

	public GameLogEventBark(Entity actor, string text, string overrideName = null, Color overrideNameColor = default(Color))
	{
		Actor = actor;
		Text = text;
		OverrideName = overrideName;
		OverrideNameColor = overrideNameColor;
	}
}
