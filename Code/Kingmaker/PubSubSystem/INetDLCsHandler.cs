using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetDLCsHandler : ISubscriber
{
	void HandleDLCsListChanged();
}
