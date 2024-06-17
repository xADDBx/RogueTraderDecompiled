using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGainFactionVendorDiscountHandler : ISubscriber
{
	void HandleGainFactionVendorDiscount(FactionType factionType, int discount);
}
