using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IViewDetachedHandler : ISubscriber<IEntity>, ISubscriber
{
	void OnViewDetached(IEntityViewBase view);
}
public interface IViewDetachedHandler<TTag> : IViewDetachedHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IViewDetachedHandler, TTag>
{
}
