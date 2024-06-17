using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Stores;

public interface IDLCStateUpdatedHandler : ISubscriber
{
	void HandleDLCUpdated();
}
