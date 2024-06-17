using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IReloadMechanicsHandler : ISubscriber
{
	void OnBeforeMechanicsReload();

	void OnMechanicsReloaded();
}
