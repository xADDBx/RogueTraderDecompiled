using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IShipCustomizationUIHandler : ISubscriber
{
	void HandleOpenShipCustomization();

	void HandleCloseAllComponentsMenu();
}
