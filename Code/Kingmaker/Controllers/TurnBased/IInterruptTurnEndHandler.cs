using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.TurnBased;

public interface IInterruptTurnEndHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitEndInterruptTurn();
}
public interface IInterruptTurnEndHandler<TTag> : IInterruptTurnEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<IInterruptTurnEndHandler, TTag>
{
}
