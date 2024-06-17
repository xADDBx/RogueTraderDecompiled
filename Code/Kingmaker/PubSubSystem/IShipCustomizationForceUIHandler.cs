using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IShipCustomizationForceUIHandler : ISubscriber
{
	void HandleForceOpenShipCustomization();

	void HandleForceCloseAllComponentsMenu();
}
