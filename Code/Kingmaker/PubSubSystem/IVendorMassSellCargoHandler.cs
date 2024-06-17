using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IVendorMassSellCargoHandler : ISubscriber
{
	void HandleMassSellChange();
}
