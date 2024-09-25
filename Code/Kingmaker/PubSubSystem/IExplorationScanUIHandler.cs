using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IExplorationScanUIHandler : ISubscriber
{
	void HandleScanCancelled();
}
