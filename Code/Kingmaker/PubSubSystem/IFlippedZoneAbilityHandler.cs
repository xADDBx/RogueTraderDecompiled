using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IFlippedZoneAbilityHandler : ISubscriber
{
	void HandleFlippedZoneAbility();
}
