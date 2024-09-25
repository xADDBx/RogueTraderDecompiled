using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitSizeHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitSizeChanged();
}
public interface IUnitSizeHandler<TTag> : IUnitSizeHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<IUnitSizeHandler, TTag>
{
}
