using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.ActivatableAbilities;

namespace Kingmaker.PubSubSystem;

public interface IActivatableAbilityWillStopHandler : ISubscriber
{
	void HandleActivatableAbilityWillStop(ActivatableAbility ability);
}
