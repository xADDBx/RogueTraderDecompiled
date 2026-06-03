using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface IAugmentationItemHandler : ISubscriber
{
	void HandleChangeItem(AugmentationsSlotVM slot);

	void HandleOverdriveSlotUpdate();
}
