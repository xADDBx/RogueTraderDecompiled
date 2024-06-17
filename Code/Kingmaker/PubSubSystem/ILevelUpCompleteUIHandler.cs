using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ILevelUpCompleteUIHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleLevelUpComplete(bool isChargen);
}
public interface ILevelUpCompleteUIHandler<TTag> : ILevelUpCompleteUIHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<ILevelUpCompleteUIHandler, TTag>
{
}
