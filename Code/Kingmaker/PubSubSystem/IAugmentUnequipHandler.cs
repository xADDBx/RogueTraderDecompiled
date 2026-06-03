using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAugmentUnequipHandler : ISubscriber
{
	void HandleAugmentUnequip();
}
