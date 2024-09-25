using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface IAbilityExecutionProcessHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleExecutionProcessStart(AbilityExecutionContext context);

	void HandleExecutionProcessEnd(AbilityExecutionContext context);
}
public interface IAbilityExecutionProcessHandler<TTag> : IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IAbilityExecutionProcessHandler, TTag>
{
}
