using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INavigatorResourceCountChangedHandler : ISubscriber
{
	void HandleChaneNavigatorResourceCount(int count);
}
