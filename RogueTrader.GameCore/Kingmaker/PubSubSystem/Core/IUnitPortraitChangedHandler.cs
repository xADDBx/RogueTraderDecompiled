using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IUnitPortraitChangedHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandlePortraitChanged();
}
public interface IUnitPortraitChangedHandler<TTag> : IUnitPortraitChangedHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitPortraitChangedHandler, TTag>
{
}
