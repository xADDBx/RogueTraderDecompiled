using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IFadeOutAndDestroyHandler : ISubscriber<IEntity>, ISubscriber
{
	void HandleFadeOutAndDestroy();
}
public interface IFadeOutAndDestroyHandler<TTag> : IFadeOutAndDestroyHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IFadeOutAndDestroyHandler, TTag>
{
}
