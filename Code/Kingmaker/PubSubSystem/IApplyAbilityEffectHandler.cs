using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace Kingmaker.PubSubSystem;

public interface IApplyAbilityEffectHandler : ISubscriber
{
	void OnAbilityEffectApplied(AbilityExecutionContext context);

	void OnTryToApplyAbilityEffect(AbilityExecutionContext context, AbilityDeliveryTarget target);

	void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target);
}
