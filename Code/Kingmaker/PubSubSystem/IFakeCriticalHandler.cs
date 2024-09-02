using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.PubSubSystem;

public interface IFakeCriticalHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleFakeCritical(RuleDealDamage evt);
}
public interface IFakeCriticalHandler<TTag> : IFakeCriticalHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IFakeCriticalHandler, TTag>
{
}
