using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitWoundHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleWoundReceived();

	void HandleWoundAvoided();
}
public interface IUnitWoundHandler<TTag> : IUnitWoundHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitWoundHandler, TTag>
{
}
