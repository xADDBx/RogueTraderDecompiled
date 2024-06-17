using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAwarenessHandler : ISubscriber<IMapObjectEntity>, ISubscriber
{
	void OnEntityNoticed(BaseUnitEntity spotter);
}
public interface IAwarenessHandler<TTag> : IAwarenessHandler, ISubscriber<IMapObjectEntity>, ISubscriber, IEventTag<IAwarenessHandler, TTag>
{
}
