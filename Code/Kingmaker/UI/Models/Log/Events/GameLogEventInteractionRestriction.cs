using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventInteractionRestriction : GameLogEvent<GameLogEventInteractionRestriction>
{
	public enum ResultType
	{
		MissingSkill,
		Jammed,
		CantDisarm
	}

	private class EventsHandler : GameLogController.GameEventsHandler, IInteractionRestrictionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
	{
		public void HandleMissingInteractionSkill(MapObjectView mapObjectView, StatType skill)
		{
			AddEvent(EventInvokerExtensions.BaseUnitEntity, mapObjectView, skill, ResultType.MissingSkill);
		}

		public void HandleJammed(MapObjectView mapObjectView)
		{
			AddEvent(null, mapObjectView, StatType.Unknown, ResultType.MissingSkill);
		}

		public void HandleCantDisarmTrap(TrapObjectView trap)
		{
			AddEvent(EventInvokerExtensions.BaseUnitEntity, trap, StatType.Unknown, ResultType.MissingSkill);
		}

		private void AddEvent(BaseUnitEntity actor, MapObjectView mapObject, StatType skill, ResultType result)
		{
			AddEvent(new GameLogEventInteractionRestriction(actor, mapObject, skill, result));
		}
	}

	public readonly BaseUnitEntity Actor;

	public readonly MapObjectView MapObject;

	public readonly StatType Skill;

	public readonly ResultType Result;

	public TrapObjectView TrapObject => MapObject as TrapObjectView;

	public GameLogEventInteractionRestriction(BaseUnitEntity actor, MapObjectView mapObject, StatType skill, ResultType result)
	{
		Actor = actor;
		MapObject = mapObject;
		Skill = skill;
		Result = result;
	}
}
