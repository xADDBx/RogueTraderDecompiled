using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IUnitBecameInvisibleHandler : ISubscriber<IEntity>, ISubscriber
{
	void OnEntityBecameInvisible();
}
public interface IUnitBecameInvisibleHandler<TTag> : IUnitBecameInvisibleHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IUnitBecameInvisibleHandler, TTag>
{
}
