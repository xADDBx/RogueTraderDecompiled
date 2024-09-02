using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventTemporaryHitPoints : GameLogEvent<GameLogEventTemporaryHitPoints>
{
	public enum States
	{
		Add,
		Remove
	}

	private class EventsHandler : GameLogController.GameEventsHandler, ITemporaryHitPoints, ISubscriber<IEntity>, ISubscriber
	{
		void ITemporaryHitPoints.HandleOnAddTemporaryHitPoints(int amount, Buff buff)
		{
			if (EventInvokerExtensions.Entity is BaseUnitEntity actor)
			{
				AddEvent(new GameLogEventTemporaryHitPoints(actor, States.Add, buff, amount));
			}
		}

		void ITemporaryHitPoints.HandleOnRemoveTemporaryHitPoints(int amount, Buff buff)
		{
			if (EventInvokerExtensions.Entity is BaseUnitEntity actor)
			{
				AddEvent(new GameLogEventTemporaryHitPoints(actor, States.Remove, buff, amount));
			}
		}
	}

	private UnitReference m_Actor;

	private States m_State;

	private Buff m_Buff;

	private int m_Amount;

	public UnitReference Actor => m_Actor;

	public States State => m_State;

	public Buff Buff => m_Buff;

	public int Amount => m_Amount;

	private GameLogEventTemporaryHitPoints(BaseUnitEntity actor, States state, Buff buff, int amount)
	{
		m_Actor = actor.FromBaseUnitEntity();
		m_State = state;
		m_Buff = buff;
		m_Amount = amount;
	}
}
