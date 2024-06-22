using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Pathfinding;

namespace Kingmaker.PubSubSystem.Core;

public interface IUnitNodeChangedHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitNodeChanged(GraphNode oldNode);
}
public interface IUnitNodeChangedHandler<TTag> : IUnitNodeChangedHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitNodeChangedHandler, TTag>
{
}
