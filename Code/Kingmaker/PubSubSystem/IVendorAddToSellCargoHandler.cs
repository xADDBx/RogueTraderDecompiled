using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IVendorAddToSellCargoHandler : ISubscriber
{
	void HandleSellChange();
}
