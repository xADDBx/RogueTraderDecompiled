using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAttackOfOpportunityHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleAttackOfOpportunity(BaseUnitEntity target);
}
public interface IAttackOfOpportunityHandler<TTag> : IAttackOfOpportunityHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IAttackOfOpportunityHandler, TTag>
{
}
