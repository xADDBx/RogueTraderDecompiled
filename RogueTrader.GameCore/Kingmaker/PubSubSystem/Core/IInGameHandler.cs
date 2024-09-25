using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IInGameHandler : ISubscriber<IEntity>, ISubscriber
{
	void HandleObjectInGameChanged();
}
public interface IInGameHandler<TTag> : IInGameHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IInGameHandler, TTag>
{
}
