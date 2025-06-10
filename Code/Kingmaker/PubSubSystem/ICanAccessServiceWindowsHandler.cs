using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICanAccessServiceWindowsHandler : ISubscriber
{
	void HandleServiceWindowsBlocked();
}
