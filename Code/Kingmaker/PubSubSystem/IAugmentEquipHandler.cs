using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAugmentEquipHandler : ISubscriber
{
	void HandleAugmentEquip(BlueprintItemAugment augmentItem);
}
