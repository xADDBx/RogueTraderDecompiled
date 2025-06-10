using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IFakeSelectHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleFakeSelected(bool value);
}
public interface IFakeSelectHandler<TTag> : IFakeSelectHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEntitySubscriber, IEventTag<IFakeSelectHandler, TTag>
{
}
