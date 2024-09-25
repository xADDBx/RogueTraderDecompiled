namespace Kingmaker.PubSubSystem.Core.Interfaces;

public interface IVendorDealHandler : ISubscriber
{
	void HandleVendorDeal();

	void HandleCancelVendorDeal();
}
