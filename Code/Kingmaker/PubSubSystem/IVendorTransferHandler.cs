using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IVendorTransferHandler : ISubscriber
{
	void HandleTransitionWindow(ItemEntity itemEntity = null);
}
