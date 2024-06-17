using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICanAccessContractsHandler : ISubscriber
{
	void HandleCanAccessContractsChanged();
}
