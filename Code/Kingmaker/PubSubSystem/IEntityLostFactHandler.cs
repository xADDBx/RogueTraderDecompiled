using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IEntityLostFactHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleEntityLostFact(EntityFact fact);
}
public interface IEntityLostFactHandler<TTag> : IEntityLostFactHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IEntityLostFactHandler, TTag>
{
}
