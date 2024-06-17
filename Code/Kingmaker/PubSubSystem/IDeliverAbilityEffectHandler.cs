using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace Kingmaker.PubSubSystem;

public interface IDeliverAbilityEffectHandler : ISubscriber
{
	void OnDeliverAbilityEffect(AbilityExecutionContext context, TargetWrapper target);
}
