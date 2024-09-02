using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IOpenLoadingScreenHandler : ISubscriber
{
	void HandleOpenLoadingScreen();
}
