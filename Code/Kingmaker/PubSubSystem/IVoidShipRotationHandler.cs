using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IVoidShipRotationHandler : ISubscriber
{
	void HandleOnRotationStart();

	void HandleOnRotationStop();
}
