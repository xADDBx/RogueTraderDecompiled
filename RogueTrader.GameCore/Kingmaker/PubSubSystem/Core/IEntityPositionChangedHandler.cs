using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IEntityPositionChangedHandler : ISubscriber<IEntity>, ISubscriber
{
	void HandleEntityPositionChanged();
}
public interface IEntityPositionChangedHandler<TTag> : IEntityPositionChangedHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IEntityPositionChangedHandler, TTag>
{
}
