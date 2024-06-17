using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IEntityGainFactHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleEntityGainFact(EntityFact fact);
}
public interface IEntityGainFactHandler<TTag> : IEntityGainFactHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IEntityGainFactHandler, TTag>
{
}
