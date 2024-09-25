using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitFactionHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleFactionChanged();
}
public interface IUnitFactionHandler<TTag> : IUnitFactionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitFactionHandler, TTag>
{
}
