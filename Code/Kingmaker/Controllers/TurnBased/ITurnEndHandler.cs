using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.TurnBased;

public interface ITurnEndHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitEndTurn(bool isTurnBased);
}
public interface ITurnEndHandler<TTag> : ITurnEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<ITurnEndHandler, TTag>
{
}
