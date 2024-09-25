using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface IPartyUseAbilityHandler : ISubscriber
{
	void HandleUseAbility(AbilityData ability);
}
