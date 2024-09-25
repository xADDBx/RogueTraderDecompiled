using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICloseLoadingScreenHandler : ISubscriber
{
	void HandleCloseLoadingScreen();
}
