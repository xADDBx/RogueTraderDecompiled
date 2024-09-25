using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Items;

public interface IVendorLogicStateChanged : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleBeginTrading();

	void HandleEndTrading();

	void HandleVendorAboutToTrading();
}
