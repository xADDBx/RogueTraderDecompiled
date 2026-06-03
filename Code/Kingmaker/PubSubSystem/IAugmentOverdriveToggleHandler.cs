using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAugmentOverdriveToggleHandler : ISubscriber
{
	void HandleAugmentOverdriveToggle(BlueprintAugmentSlot currentSlot, int layer);
}
