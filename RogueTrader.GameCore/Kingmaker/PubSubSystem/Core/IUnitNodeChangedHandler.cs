using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IUnitNodeChangedHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitNodeChanged();
}
public interface IUnitNodeChangedHandler<TTag> : IUnitNodeChangedHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitNodeChangedHandler, TTag>
{
}
