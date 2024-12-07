using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IFlipZoneAbilityHandler : ISubscriber
{
	void HandleFlipZoneAbility();
}
