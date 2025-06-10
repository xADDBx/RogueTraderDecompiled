using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICanAccessSelectedWindowsHandler : ISubscriber
{
	void HandleSelectedWindowsBlocked();
}
