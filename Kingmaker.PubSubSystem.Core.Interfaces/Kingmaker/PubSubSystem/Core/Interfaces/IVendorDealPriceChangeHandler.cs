namespace Kingmaker.PubSubSystem.Core.Interfaces;

public interface IVendorDealPriceChangeHandler : ISubscriber
{
	void HandleDealPriceChanged(float dealPrice);
}
