using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventHealWoundOrTrauma : GameLogEvent<GameLogEventHealWoundOrTrauma>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IHealWoundOrTrauma, ISubscriber<IEntity>, ISubscriber
	{
		void IHealWoundOrTrauma.HandleOnHealWoundOrTrauma(Buff buff)
		{
			if (EventInvokerExtensions.Entity is BaseUnitEntity actor)
			{
				AddEvent(new GameLogEventHealWoundOrTrauma(actor, buff));
			}
		}
	}

	private UnitReference m_Actor;

	private Buff m_Buff;

	public UnitReference Actor => m_Actor;

	public Buff Buff => m_Buff;

	private GameLogEventHealWoundOrTrauma(BaseUnitEntity actor, Buff buff)
	{
		m_Actor = actor.FromBaseUnitEntity();
		m_Buff = buff;
	}
}
