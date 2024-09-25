using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UnitLogic.Abilities.Components;

public interface IDirectMovementHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleDirectMovementStarted(ForcedPath path, bool disableAttacksOfOpportunity);

	void HandleDirectMovementEnded();
}
public interface IDirectMovementHandler<TTag> : IDirectMovementHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IDirectMovementHandler, TTag>
{
}
