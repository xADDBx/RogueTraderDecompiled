using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitTraumaHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleTraumaReceived();
}
public interface IUnitTraumaHandler<TTag> : IUnitTraumaHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitTraumaHandler, TTag>
{
}
