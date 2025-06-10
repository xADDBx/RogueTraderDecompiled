using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface IAbilityExecutionProcessRedirectHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleAbilityRedirected(AbilityExecutionContext context);
}
public interface IAbilityExecutionProcessRedirectHandler<TTag> : IAbilityExecutionProcessRedirectHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IAbilityExecutionProcessRedirectHandler, TTag>
{
}
