using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface IUnitAbilityCooldownHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleAbilityCooldownStarted(AbilityData ability);

	void HandleGroupCooldownRemoved(BlueprintAbilityGroup group);

	void HandleCooldownReset();
}
public interface IUnitAbilityCooldownHandler<TTag> : IUnitAbilityCooldownHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitAbilityCooldownHandler, TTag>
{
}
