using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IResourceAmountBonusHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus);
}
public interface IResourceAmountBonusHandler<TTag> : IResourceAmountBonusHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IResourceAmountBonusHandler, TTag>
{
}
