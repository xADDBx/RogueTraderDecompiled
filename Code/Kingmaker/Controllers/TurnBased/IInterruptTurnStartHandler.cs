using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.TurnBased;

public interface IInterruptTurnStartHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitStartInterruptTurn(InterruptionData interruptionData);
}
public interface IInterruptTurnStartHandler<TTag> : IInterruptTurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<IInterruptTurnStartHandler, TTag>
{
}
