using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IUnitBecameVisibleHandler : ISubscriber<IEntity>, ISubscriber
{
	void OnEntityBecameVisible();
}
public interface IUnitBecameVisibleHandler<TTag> : IUnitBecameVisibleHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IUnitBecameVisibleHandler, TTag>
{
}
