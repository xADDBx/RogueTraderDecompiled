using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IVendorBuyHandler : ISubscriber
{
	void HandleBuyItem(ItemEntity buyingItem);
}
