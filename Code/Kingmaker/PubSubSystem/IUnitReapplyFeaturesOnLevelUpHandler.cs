using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitReapplyFeaturesOnLevelUpHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitReapplyFeaturesOnLevelUp();
}
public interface IUnitReapplyFeaturesOnLevelUpHandler<TTag> : IUnitReapplyFeaturesOnLevelUpHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitReapplyFeaturesOnLevelUpHandler, TTag>
{
}
